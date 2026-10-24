using Manager;
using System;
using System.Collections.Generic;
using System.IO;
using KKAPI.Chara;
using UnityEngine;
using static PseudoMaker.PseudoMaker;

namespace PseudoMaker
{
    /// <summary>
    /// Saves the selected character as a character card, or its current outfit as a coordinate card.
    /// Cards are written to UserData/chara/pseudomaker and UserData/coordinate/pseudomaker.
    /// </summary>
    public static class CardSaver
    {
        // Same dimensions the maker itself uses for card thumbnails, see ChaCustom.CustomCapture.CapCharaCard
        private const int CardWidth = 252;
        private const int CardHeight = 352;

        /// <summary>
        /// Saves the selected character as a character card.
        /// </summary>
        /// <param name="useGameMethod">
        /// When true the game's own ChaFileControl.SaveCharaFile is used, which also assigns a new
        /// dataID/userID and updates charaFileName. When false the card is written through the plain
        /// ChaFile.SaveFile, leaving all of that metadata untouched.
        /// </param>
        public static void SaveCharacter(bool useGameMethod)
        {
            ChaControl chaControl = selectedCharacter;
            if (!chaControl)
            {
                PseudoMaker.Logger.LogMessage("No character selected");
                return;
            }

            try
            {
                ChaFileControl chaFile = chaControl.chaFile;
                string subfolder = chaFile.parameter.sex == 0 ? "male" : "female";
                string path = GetSavePath($"chara/{subfolder}", $"PseudoMakerChara_{chaFile.GetFancyCharacterName()}");

                chaFile.pngData = CreatePlaceholderCardImage();

                if (useGameMethod)
                {
                    if (!chaFile.SaveCharaFile(path, 255, true))
                    {
                        PseudoMaker.Logger.LogMessage("Failed to save character card");
                        return;
                    }
                }
                else
                {
                    chaFile.SaveFile(path);
                }

                PseudoMaker.Logger.LogMessage($"Saved character card to {path}");
            }
            catch (Exception e)
            {
                PseudoMaker.Logger.LogMessage("Failed to save character card - " + e.Message);
                PseudoMaker.Logger.LogError(e);
            }
        }

        /// <summary>
        /// Saves the outfit the selected character is currently wearing as a coordinate card.
        /// </summary>
        /// <param name="name">Name for the coordinate, leave empty to use filename instead.</param>
        public static void SaveCoordinate(string name = "")
        {
            
            ChaControl chaControl = selectedCharacter;
            if (!chaControl)
            {
                PseudoMaker.Logger.LogMessage("No character selected");
                return;
            }

            try
            {
                string path = GetSavePath("coordinate", "PseudoMakerCoordinate");
                ChaFileCoordinate coordinate = chaControl.nowCoordinate;

                if (name.IsNullOrWhiteSpace())
                {
                    name = Path.GetFileNameWithoutExtension(path);
                }
                
                coordinate.pngData = CreatePlaceholderCardImage();
                coordinate.coordinateName = name;

                coordinate.SaveFile(path);

                PseudoMaker.Logger.LogMessage($"Saved coordinate card to {path}");
            }
            catch (Exception e)
            {
                PseudoMaker.Logger.LogMessage("Failed to save coordinate card - " + e.Message);
                PseudoMaker.Logger.LogError(e);
            }
        }

        private static string GetSavePath(string cardType, string filePrefix)
        {
            var directory = Path.Combine(Path.Combine(UserData.Path, cardType), "pseudomaker");
            Directory.CreateDirectory(directory);

            var path = Path.Combine(directory, $"{filePrefix}_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.png");
            // Normalize, the game builds its own paths with forward slashes
            return Path.GetFullPath(path);
        }

        /// <summary>
        /// Builds the image embedded in the card, which is what card lists show as the thumbnail.
        /// Studio has no card capture camera, so for now this is a placeholder stating when the card
        /// was exported. Replace this with a real screenshot once that is wired up.
        /// </summary>
        private static byte[] CreatePlaceholderCardImage()
        {
            Texture2D texture = null;
            try
            {
                texture = new Texture2D(CardWidth, CardHeight, TextureFormat.ARGB32, false);

                var pixels = new Color32[CardWidth * CardHeight];
                var background = new Color32(0, 0, 0, 255);
                for (var i = 0; i < pixels.Length; i++)
                    pixels[i] = background;

                var text = new string[]
                {
                    "PSEUDO MAKER",
                    "EXPORT AT",
                    $"{DateTime.Now:yyyy-MM-dd}",
                    $"{DateTime.Now:HH:mm:ss}",
                };

                // Centered block of text, drawn a little above the middle of the card
                const int scale = 3;
                const int lineSpacing = (GlyphHeight + 2) * scale;
                var blockHeight = text.Length * lineSpacing;
                var y = (CardHeight + blockHeight) / 2 + 40;

                foreach (var line in text)
                {
                    y -= lineSpacing;
                    var lineWidth = line.Length * (GlyphWidth + 1) * scale;
                    DrawText(pixels, line, (CardWidth - lineWidth) / 2, y, scale);
                }

                texture.SetPixels32(pixels);
                texture.Apply();

                return texture.EncodeToPNG();
            }
            finally
            {
                if (texture != null)
                    UnityEngine.Object.Destroy(texture);
            }
        }

        #region Text rendering
        // Unity's font APIs need a live UI canvas to rasterize through, which the placeholder does not
        // warrant, so the few characters used above are drawn from this 5x7 bitmap font instead.
        private const int GlyphWidth = 5;
        private const int GlyphHeight = 7;

        private static readonly Dictionary<char, string> Glyphs = new Dictionary<char, string>
        {
            { 'A', "01110 10001 10001 11111 10001 10001 10001" },
            { 'C', "01110 10001 10000 10000 10000 10001 01110" },
            { 'D', "11110 10001 10001 10001 10001 10001 11110" },
            { 'E', "11111 10000 10000 11110 10000 10000 11111" },
            { 'G', "01110 10001 10000 10111 10001 10001 01110" },
            { 'K', "10001 10010 10100 11000 10100 10010 10001" },
            { 'M', "10001 11011 10101 10101 10001 10001 10001" },
            { 'O', "01110 10001 10001 10001 10001 10001 01110" },
            { 'P', "11110 10001 10001 11110 10000 10000 10000" },
            { 'R', "11110 10001 10001 11110 10100 10010 10001" },
            { 'S', "01111 10000 10000 01110 00001 00001 11110" },
            { 'T', "11111 00100 00100 00100 00100 00100 00100" },
            { 'U', "10001 10001 10001 10001 10001 10001 01110" },
            { 'X', "10001 10001 01010 00100 01010 10001 10001" },
            { '0', "01110 10001 10011 10101 11001 10001 01110" },
            { '1', "00100 01100 00100 00100 00100 00100 01110" },
            { '2', "01110 10001 00001 00010 00100 01000 11111" },
            { '3', "11110 00001 00001 01110 00001 00001 11110" },
            { '4', "00010 00110 01010 10010 11111 00010 00010" },
            { '5', "11111 10000 11110 00001 00001 10001 01110" },
            { '6', "00110 01000 10000 11110 10001 10001 01110" },
            { '7', "11111 00001 00010 00100 01000 01000 01000" },
            { '8', "01110 10001 10001 01110 10001 10001 01110" },
            { '9', "01110 10001 10001 01111 00001 00010 01100" },
            { '-', "00000 00000 00000 01110 00000 00000 00000" },
            { ':', "00000 00100 00000 00000 00000 00100 00000" },
            { ' ', "00000 00000 00000 00000 00000 00000 00000" },
        };

        private static void DrawText(Color32[] pixels, string text, int x, int y, int scale)
        {
            var advance = (GlyphWidth + 1) * scale;
            foreach (var character in text)
            {
                DrawGlyph(pixels, character, x, y, scale);
                x += advance;
            }
        }

        private static void DrawGlyph(Color32[] pixels, char character, int x, int y, int scale)
        {
            if (!Glyphs.TryGetValue(character, out var glyph)) return;

            var rows = glyph.Split(' ');
            var foreground = new Color32(255, 255, 255, 255);

            for (int row = 0; row < rows.Length; row++)
            {
                for (int column = 0; column < rows[row].Length; column++)
                {
                    if (rows[row][column] != '1') continue;

                    // Textures are addressed bottom up, the glyph rows are listed top down
                    for (int blockY = 0; blockY < scale; blockY++)
                    {
                        var pixelY = y + (GlyphHeight - 1 - row) * scale + blockY;
                        if (pixelY < 0 || pixelY >= CardHeight) continue;

                        for (int blockX = 0; blockX < scale; blockX++)
                        {
                            var pixelX = x + column * scale + blockX;
                            if (pixelX < 0 || pixelX >= CardWidth) continue;

                            pixels[pixelY * CardWidth + pixelX] = foreground;
                        }
                    }
                }
            }
        }
        #endregion
    }
}
