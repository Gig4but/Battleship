/************************************************
 * Lode, console game "Ship Battle"
 *
 * Created by Oleg Petruny in 2022
 * like credit program for Programming course
 * 
 * License MIT
 ************************************************/

using static Lode.Graphics;

namespace Lode {
    /// <summary>Represents static collection of Sixel sprites.</summary>
    public class Sprites {
        /// <summary>Sprite of palm tree.</summary>
        public static readonly List<List<Sixel>> Tree = new List<List<Sixel>>
        {
            new List<Sixel> { },
            new List<Sixel> { new(14, Color.Transparent), new("_ _", Color.Green) },
            new List<Sixel> { new(13, Color.Transparent), new("/   |", Color.Green) },
            new List<Sixel> { new(5, Color.Transparent), new("_ _ _", Color.Green), new(2, Color.Transparent), new("/     |", Color.Green) },
            new List<Sixel> { new(4, Color.Transparent), new(@"/     \/_ /", Color.Green), new(1, Color.Transparent), new(@"\  |", Color.Green) },
            new List<Sixel> { new(3, Color.Transparent), new("|   /       |", Color.Green), new(1, Color.Transparent), new(@"\  |", Color.Green) },
            new List<Sixel> { new(2, Color.Transparent), new("|   /", Color.Green), new(1, Color.Transparent), new("|   |", Color.Yellow), new("   |", Color.Green), new(1, Color.Transparent), new(@"\  |", Color.Green)},
            new List<Sixel> { new(1, Color.Transparent), new("|   /", Color.Green), new(1, Color.Transparent), new("|_ _", Color.Yellow), new("C C", Color.Brown), new("   |", Color.Green), new(1, Color.Transparent), new(@"\/", Color.Green) },
            new List<Sixel> { new(1, Color.Transparent), new(@"\  /", Color.Green), new(1, Color.Transparent), new("|_ _", Color.Yellow), new("C   C", Color.Brown), new("   |", Color.Green) },
            new List<Sixel> { new(2, Color.Transparent), new(@"\/", Color.Green), new(1, Color.Transparent), new("|   |", Color.Yellow), new(1, Color.Transparent), new("C C", Color.Brown), new(1, Color.Transparent), new(@"\  /", Color.Green) },
            new List<Sixel> { new(4, Color.Transparent), new("|_ _|", Color.Yellow), new(7, Color.Transparent), new(@"\/", Color.Green) },
            new List<Sixel> { new(3, Color.Transparent), new("|_ _|", Color.Yellow) },
            new List<Sixel> { new(3, Color.Transparent), new("|   |", Color.Yellow) },
            new List<Sixel> { new(3, Color.Transparent), new("|_ _|", Color.Yellow) },
            new List<Sixel> { new(3, Color.Transparent), new("|   |", Color.Yellow) },
            new List<Sixel> { new(3, Color.Transparent), new("|_ _|", Color.Yellow) },
            new List<Sixel> { new(3, Color.Transparent), new("|_ _|", Color.Yellow) },
            new List<Sixel> { new(3, Color.Transparent), new("|_ _|", Color.Yellow) },
            new List<Sixel> { new(3, Color.Transparent), new("|   |", Color.Yellow) },
            new List<Sixel> { new(1, Color.Transparent), new(".,", Color.DarkGreen), new("|   |", Color.Yellow), new(",.,.,,.,.,..", Color.DarkGreen) },
            new List<Sixel> { new(1, Color.Transparent), new("                   \"',", Color.DarkGreen) },
            new List<Sixel> { new(1, Color.Transparent), new(2, Color.Brown), new(6, Color.DarkGreen), new(5, Color.Brown), new(4, Color.Brown), new("     \";", Color.DarkGreen) },
            new List<Sixel> { new(1, Color.Transparent), new(22, Color.Brown), new("  7", Color.Yellow) },
            new List<Sixel> { new(1, Color.Transparent), new(11, Color.Gray), new(2, Color.Brown), new(10, Color.Brown), new(" J", Color.Yellow) },
            new List<Sixel> { new(1, Color.Transparent), new(17, Color.Gray), new(4, Color.Brown), new("    3", Color.Gray) },
            new List<Sixel> { new(1, Color.Transparent), new(1, Color.Gray), new(1, Color.DarkGray), new(2, Color.Gray), new(4, Color.DarkGray),  new("                  3", Color.Gray) },
            new List<Sixel> { new(1, Color.Transparent), new(11, Color.DarkGray), new(9, Color.Gray), new("      3", Color.DarkGray) },
            new List<Sixel> { new(1, Color.Transparent), new("                         3", Color.DarkGray) },
        };

        /// <summary>Sprite of ship.</summary>
        public static readonly List<List<Sixel>>[] Ship = new List<List<Sixel>>[]
        {
            new List<List<Sixel>> {
                new List<Sixel> { new(7, Color.Transparent), new("_ _ _ _ _", Color.Gray) },
                new List<Sixel> { new(6, Color.Transparent), new(@"/         \", Color.Gray) },
                new List<Sixel> { new(@"_ _ _/_ _ _ _ _ _\_ _ _", Color.Gray) },
                new List<Sixel> { new(@"\                     /", Color.Gray) },
                new List<Sixel> { new(1, Color.Transparent), new(@"\  * * * * * * * *  /", Color.DarkGray) },
                new List<Sixel> { new(2, Color.Transparent), new(@"\                 /", Color.DarkGray) },
                new List<Sixel> { new(3, Color.Transparent), new(@"\_ _ _ _ _ _ _ _/", Color.DarkGray) },
            },
            new List<List<Sixel>> {
                new List<Sixel> { new(7, Color.Transparent), new("_ _ _ _ _", Color.Gray) },
                new List<Sixel> { new(6, Color.Transparent), new(@"/         \", Color.Gray) },
                new List<Sixel> { new(@"_ _ _/_ _ _ _ _ _\_ _ _", Color.Gray) },
                new List<Sixel> { new(@"\                     /", Color.Gray) },
                new List<Sixel> { new(1, Color.Transparent), new(@"\  * * * * * * * *  /", Color.Gray) },
                new List<Sixel> { new(2, Color.Transparent), new(@"\                 /", Color.DarkGray) },
                new List<Sixel> { new(3, Color.Transparent), new(@"\_ _ _ _ _ _ _ _/", Color.DarkGray) },
            },
        };

        /// <summary>Sprite of golden fish.</summary>
        public static readonly List<List<Sixel>>[] Fish = new List<List<Sixel>>[]
        {
            new List<List<Sixel>> {
                new List<Sixel> { new(@"|\/   * \", Color.Yellow), },
                new List<Sixel> { new(@"|/\_ _ _/", Color.Yellow) },
            },
            new List<List<Sixel>> {
                new List<Sixel> { new(@"/ *   \/|", Color.Yellow), },
                new List<Sixel> { new(@"\_ _ _/\|", Color.Yellow) },
            },
        };

    }
}
