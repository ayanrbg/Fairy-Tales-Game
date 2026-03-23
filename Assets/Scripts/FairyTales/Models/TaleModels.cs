using System;

namespace FairyTales.Models
{
    [Serializable]
    public class TaleSummary
    {
        public string id;
        public string title;
        public string lang;
        public string file;
        public bool hasDefaultNarration;
        public string coverUrl;
    }

    [Serializable]
    public class TaleDetail
    {
        public string id;
        public string title;
        public string lang;
        public int totalPages;
        public string[] pages;
    }

    [Serializable]
    public class PersonalizeRequest
    {
        public string name;
        public string gender;
    }

    [Serializable]
    public class PersonalizeResponse
    {
        public string[] pages;
    }

    [Serializable]
    public class DefaultNarrationInfo
    {
        public bool available;
        public string lang;
        public int[] pages;
    }
}
