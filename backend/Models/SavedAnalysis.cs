namespace FakeNewsDetector.Models
{
    public class SavedAnalysis
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public double Score { get; set; }
        public string Verdict { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        
        public string FormattedDate => Date.ToString("MMM dd, yyyy HH:mm");
        
        public string RelativeDate
        {
            get
            {
                var timeSpan = DateTime.UtcNow - Date;
                
                if (timeSpan.TotalMinutes < 1)
                    return "Just now";
                if (timeSpan.TotalHours < 1)
                    return $"{(int)timeSpan.TotalMinutes} minutes ago";
                if (timeSpan.TotalDays < 1)
                    return $"{(int)timeSpan.TotalHours} hours ago";
                if (timeSpan.TotalDays < 7)
                    return $"{(int)timeSpan.TotalDays} days ago";
                
                return Date.ToString("MMM dd, yyyy");
            }
        }
    }
}
