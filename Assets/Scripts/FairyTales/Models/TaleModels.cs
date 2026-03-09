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
}
