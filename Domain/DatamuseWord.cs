using System;
using System.Collections.Generic;
using System.Text;

namespace Domain;

// Only used for Datamuse API response
public class DatamuseWord
{
    public string Word { get; set; } = "";
    public int Score { get; set; }
    public List<string> Tags { get; set; } = new();
}
