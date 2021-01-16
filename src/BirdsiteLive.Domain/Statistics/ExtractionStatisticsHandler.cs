using System.Threading;
using System.Timers;

namespace BirdsiteLive.Domain.Statistics
{
    public interface IExtractionStatisticsHandler
    {
        void ExtractedDescription(int mentionsCount);
        void ExtractedStatus(int mentionsCount);
        ExtractionStatistics GetStatistics();
    }

    public class ExtractionStatisticsHandler : IExtractionStatisticsHandler
    {
        private static int _lastDescriptionMentionsExtracted;
        private static int _lastStatusMentionsExtracted;

        private static int _descriptionMentionsExtracted;
        private static int _statusMentionsExtracted;

        private static System.Timers.Timer _resetTimer;

        #region Ctor
        public ExtractionStatisticsHandler()
        {
            if (_resetTimer == null)
            {
                _resetTimer = new System.Timers.Timer();
                _resetTimer.Elapsed += OnTimeResetEvent;
                _resetTimer.Interval = 24 * 60 * 60 * 1000; // 24h
                _resetTimer.Enabled = true;
            }
        }
        #endregion

        private void OnTimeResetEvent(object sender, ElapsedEventArgs e)
        {
            _lastDescriptionMentionsExtracted = _descriptionMentionsExtracted;
            _lastStatusMentionsExtracted = _statusMentionsExtracted;

            // Reset
            Interlocked.Exchange(ref _descriptionMentionsExtracted, 0);
            Interlocked.Exchange(ref _statusMentionsExtracted, 0);
        }

        public void ExtractedDescription(int mentionsCount)
        {
            for (var i = 0; i < mentionsCount; i++)
                Interlocked.Increment(ref _descriptionMentionsExtracted);
        }

        public void ExtractedStatus(int mentionsCount)
        {
            for (var i = 0; i < mentionsCount; i++)
                Interlocked.Increment(ref _statusMentionsExtracted);
        }

        public ExtractionStatistics GetStatistics()
        {
            return new ExtractionStatistics(_descriptionMentionsExtracted, _statusMentionsExtracted, _lastDescriptionMentionsExtracted, _lastStatusMentionsExtracted);
        }
    }

    public class ExtractionStatistics
    {
        #region Ctor
        public ExtractionStatistics(int mentionsInDescriptionsExtraction, int mentionsInStatusesExtraction, int lastMentionsInDescriptionsExtraction, int lastMentionsInStatusesExtraction)
        {
            MentionsInDescriptionsExtraction = mentionsInDescriptionsExtraction;
            MentionsInStatusesExtraction = mentionsInStatusesExtraction;
            LastMentionsInDescriptionsExtraction = lastMentionsInDescriptionsExtraction;
            LastMentionsInStatusesExtraction = lastMentionsInStatusesExtraction;
        }
        #endregion

        public int MentionsInDescriptionsExtraction { get; }
        public int MentionsInStatusesExtraction { get; }

        public int LastMentionsInDescriptionsExtraction { get; }
        public int LastMentionsInStatusesExtraction { get; }
    }
}