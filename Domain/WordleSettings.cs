using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Domain;

public class WordleSettings
{
    public WordleSettings()
    {
        WordLength = 5;
        MaxGuesses = 6;
        HardMode = false;
        IsKeyboardActive = true;
    }

    public string? CurrentUsername { get; set; }

    public int WordLength { get; set; }

    public int MaxGuesses { get; set; }

    public bool HardMode { get; set; }

    public bool IsKeyboardActive { get; set; }
}
