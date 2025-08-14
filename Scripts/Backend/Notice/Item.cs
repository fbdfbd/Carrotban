using System;

namespace BackendData.Notice
{
    public class Item
    { 
        public string Title { get; private set; }
        public string Date { get; private set; }
        public string Content { get; private set; }

        public Item(string title, string date, string content)
        {
            Title = title;
            Date = date;
            Content = content;
        }

        public override string ToString()
        {
            return $"Title: {Title}\nDate: {Date}\nContent: {Content}\n";
        }
    }
}
