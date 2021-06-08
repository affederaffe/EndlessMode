using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EndlessMode.Configuration;

using IPA.Utilities;

using SiraUtil.Services;

using UnityEngine;
using UnityEngine.Scripting;

using Zenject;


namespace EndlessMode
{
    internal class EndlessSongController : GameSongController, IInitializable, IDisposable
    {
        private PluginConfig _config = null!;
        private Submission _submission = null!;
        private BeatmapLevelsModel _beatmapLevelsModel = null!;
        private GameplayCoreSceneSetupData _gameplayCoreSceneSetupData = null!;
        private GameSongController _gameSongController = null!;

        private System.Random _random = null!;
        private Submission.Ticket _ticket = null!;
        private IPreviewBeatmapLevel[] _previewBeatmapLevels = null!;

        [Inject]
        public void Construct(PluginConfig config,
                              Submission submission,
                              AudioTimeSyncController audioTimeSyncController,
                              BeatmapLevelsModel beatmapLevelsModel,
                              GameplayCoreSceneSetupData gameplayCoreSceneSetupData,
                              GameSongController gameSongController)
        {
            _config = config;
            _submission = submission;
            _audioTimeSyncController = audioTimeSyncController;
            _beatmapLevelsModel = beatmapLevelsModel;
            _gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
            _gameSongController = gameSongController;
            _random = new System.Random();
            IBeatmapLevelPack beatmapLevelPack = _beatmapLevelsModel.GetLevelPackForLevelId(_gameplayCoreSceneSetupData.difficultyBeatmap.level.levelID);
            _previewBeatmapLevels = beatmapLevelPack.beatmapLevelCollection.beatmapLevels;
        }

        public void Initialize()
        {
            GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
            _ticket = _submission.DisableScoreSubmission("EndlessMode");
        }

        public void Dispose()
        {
            _submission.Remove(_ticket);
        }

        public override void LateUpdate()
        {
            if (_config.Enabled && !_songDidFinish && _audioTimeSyncController.songTime >= _audioTimeSyncController.songEndTime - 0.2f)
            {
                _songDidFinish = true;
                StartNewSong();
            }
            else
            {
                base.LateUpdate();
            }
        }

        private async void StartNewSong()
        {
            IDifficultyBeatmap difficultyBeatmap = await NextDifficultyBeatmap();
            _gameplayCoreSceneSetupData.SetField("difficultyBeatmap", difficultyBeatmap);
            AudioTimeSyncController.InitData audioInitData = new(difficultyBeatmap.level.beatmapLevelData.audioClip, 0f, difficultyBeatmap.level.songTimeOffset, _audioTimeSyncController.timeScale);
            _audioTimeSyncController.audioSource.clip.UnloadAudioData();
            _audioTimeSyncController.Awake();
            _audioTimeSyncController.SetField("_initData", audioInitData);
            _audioTimeSyncController.SetField("_audioStarted", false);
            _audioTimeSyncController.Start();
            BeatmapObjectCallbackController.InitData callbackInitData = new(difficultyBeatmap.beatmapData, 0f);
            // ReSharper disable once Unity.UnresolvedComponentOrScriptableObject
            Component? customCallbackController = ((BeatmapObjectCallbackController)_beatmapObjectCallbackController).gameObject.GetComponent("CustomEventCallbackController");
            if (customCallbackController is not null) Destroy(customCallbackController);
            ((BeatmapObjectCallbackController)_beatmapObjectCallbackController).SetField("_initData", callbackInitData);
            ((BeatmapObjectCallbackController)_beatmapObjectCallbackController).SetField("_beatmapData", (IReadonlyBeatmapData?)null);
            ((BeatmapObjectCallbackController)_beatmapObjectCallbackController).Start();
            _gameSongController.StartSong();
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        private async Task<IDifficultyBeatmap> NextDifficultyBeatmap()
        {
            IPreviewBeatmapLevel previewBeatmapLevel = _previewBeatmapLevels[_random.Next(0, _previewBeatmapLevels.Length)];
            BeatmapLevelsModel.GetBeatmapLevelResult getBeatmapLevelResult = await _beatmapLevelsModel.GetBeatmapLevelAsync(previewBeatmapLevel.levelID, CancellationToken.None);
            if (getBeatmapLevelResult.isError) return await NextDifficultyBeatmap();
            IDifficultyBeatmapSet? difficultyBeatmapSet = getBeatmapLevelResult.beatmapLevel.beatmapLevelData.difficultyBeatmapSets.FirstOrDefault(x => x.beatmapCharacteristic.serializedName == _gameplayCoreSceneSetupData.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName)
                                                          ?? getBeatmapLevelResult.beatmapLevel.beatmapLevelData.difficultyBeatmapSets.First();
            IDifficultyBeatmap difficultyBeatmap = difficultyBeatmapSet.difficultyBeatmaps.FirstOrDefault(x => x.difficultyRank <= _gameplayCoreSceneSetupData.difficultyBeatmap.difficultyRank)
                                                   ?? difficultyBeatmapSet.difficultyBeatmaps.Last(x => x.difficultyRank > _gameplayCoreSceneSetupData.difficultyBeatmap.difficultyRank);
            string[]? requirements = SongCore.Collections.RetrieveExtraSongData(previewBeatmapLevel.levelID)?._difficulties?.First(x => x._difficulty == difficultyBeatmap.difficulty).additionalDifficultyData._requirements;
            return requirements?.All(x => SongCore.Collections.capabilities.Contains(x)) ?? true ? difficultyBeatmap : await NextDifficultyBeatmap();
        }
    }
}