using System;
using System.Text;

namespace SteamBolt;

public static class RandomNameGenerator
{
    private static readonly string[] Consonants =
    {
        "b","c","d","f","g","h","j","k","l","m","n",
        "p","r","s","t","v","w","z",
        "br","cr","dr","fr","gr","kr","pr","tr",
        "ch","sh","th"
    };

    private static readonly string[] Vowels =
    {
        "a","e","i","o","u","ae","ai","ea","ie","oa","ou"
    };

    private static readonly Random Rng = new();

    public static string Generate(int minSyllables = 2, int maxSyllables = 4)
    {
        int syllables = Rng.Next(minSyllables, maxSyllables + 1);
        var sb = new StringBuilder();

        for (int i = 0; i < syllables; i++)
        {
            sb.Append(Consonants[Rng.Next(Consonants.Length)]);
            sb.Append(Vowels[Rng.Next(Vowels.Length)]);
        }

        sb[0] = char.ToUpperInvariant(sb[0]);
        return sb.ToString();
    }
}