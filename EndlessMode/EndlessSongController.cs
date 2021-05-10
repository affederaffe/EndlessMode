using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EndlessMode.HarmonyPatches;

using IPA.Utilities;

using SiraUtil.Services;

using Zenject;


namespace EndlessMode
{
    internal class EndlessSongController : IInitializable, IDisposable
    {
        private readonly Submission _submission;
        private readonly AudioTimeSyncController _audioTimeSyncController;
        private readonly BeatmapLevelsModel _beatmapLevelsModel;
        private readonly GameplayCoreSceneSetupData _gameplayCoreSceneSetupData;
        private readonly GameSongController _gameSongController;
        private readonly IBeatmapObjectCallbackController _beatmapObjectCallbackController;
        private readonly Random _random;

        private IPreviewBeatmapLevel[]? _previewBeatmapLevels;

        internal EndlessSongController(Submission submission,
            AudioTimeSyncController audioTimeSyncController,
            BeatmapLevelsModel beatmapLevelsModel,
            GameplayCoreSceneSetupData gameplayCoreSceneSetupData,
            GameSongController gameSongController,
            IBeatmapObjectCallbackController beatmapObjectCallbackController)
        {
            _submission = submission;
            _audioTimeSyncController = audioTimeSyncController;
            _beatmapLevelsModel = beatmapLevelsModel;
            _gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
            _gameSongController = gameSongController;
            _beatmapObjectCallbackController = beatmapObjectCallbackController;
            _random = new Random();
        }

        public void Initialize()
        {
            _submission.DisableScoreSubmission("EndlessMode");
            IBeatmapLevelPack beatmapLevelPack = _beatmapLevelsModel.GetLevelPackForLevelId(_gameplayCoreSceneSetupData.difficultyBeatmap.level.levelID);
            _previewBeatmapLevels = beatmapLevelPack.beatmapLevelCollection.beatmapLevels;
            SongControllerPatch.SongDidFinishEvent += OnSongDidFinishEvent;
        }

        public void Dispose()
        {
            SongControllerPatch.SongDidFinishEvent -= OnSongDidFinishEvent;
        }

        private async void OnSongDidFinishEvent()
        {
            IDifficultyBeatmap difficultyBeatmap = await NextDifficultyBeatmap();
            _gameplayCoreSceneSetupData.SetField("difficultyBeatmap", difficultyBeatmap);
            AudioTimeSyncController.InitData audioInitData = new(difficultyBeatmap.level.beatmapLevelData.audioClip, 0f, difficultyBeatmap.level.songTimeOffset, _audioTimeSyncController.timeScale);
            _audioTimeSyncController.Awake();
            _audioTimeSyncController.SetField("_initData", audioInitData);
            _audioTimeSyncController.SetField("_audioStarted", false);
            _audioTimeSyncController.Start();
            BeatmapObjectCallbackController.InitData callbackInitData = new(difficultyBeatmap.beatmapData, 0f);
            ((BeatmapObjectCallbackController)_beatmapObjectCallbackController).SetField("_initData", callbackInitData);
            ((BeatmapObjectCallbackController)_beatmapObjectCallbackController).SetField("_beatmapData", (IReadonlyBeatmapData?)null);
            ((BeatmapObjectCallbackController)_beatmapObjectCallbackController).Start();
            _gameSongController.StartSong();
        }

        private async Task<IDifficultyBeatmap> NextDifficultyBeatmap()
        {
            IPreviewBeatmapLevel previewBeatmapLevel = _previewBeatmapLevels![_random.Next(0, _previewBeatmapLevels.Length)];
            BeatmapLevelsModel.GetBeatmapLevelResult getBeatmapLevelResult = await _beatmapLevelsModel.GetBeatmapLevelAsync(previewBeatmapLevel.levelID, CancellationToken.None);
            IDifficultyBeatmapSet? difficultyBeatmapSet = getBeatmapLevelResult.beatmapLevel.beatmapLevelData.difficultyBeatmapSets.FirstOrDefault(x => x.beatmapCharacteristic.serializedName == _gameplayCoreSceneSetupData.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName)
                                                          ?? getBeatmapLevelResult.beatmapLevel.beatmapLevelData.difficultyBeatmapSets.First();
            IDifficultyBeatmap difficultyBeatmap = difficultyBeatmapSet.difficultyBeatmaps.FirstOrDefault(x => x.difficultyRank <= _gameplayCoreSceneSetupData.difficultyBeatmap.difficultyRank)
                                                   ?? difficultyBeatmapSet.difficultyBeatmaps.Last(x => x.difficultyRank > _gameplayCoreSceneSetupData.difficultyBeatmap.difficultyRank);
            string[]? requirements = SongCore.Collections.RetrieveExtraSongData(previewBeatmapLevel.levelID)?._difficulties?
                .First(x => x._difficulty == difficultyBeatmap.difficulty).additionalDifficultyData._requirements;
            return requirements?.All(x => SongCore.Collections.capabilities.Contains(x)) ?? true ? difficultyBeatmap : await NextDifficultyBeatmap();
        }
    }
}