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
        IsKeyboardActive = true;
    }

    public string? CurrentUsername { get; set; }

    public int WordLength { get; set; }

    public int MaxGuesses { get; set; }

    public bool IsKeyboardActive { get; set; }
}
