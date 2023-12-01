using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using RTFunctions.Functions;
using RTFunctions.Functions.IO;
using RTFunctions.Functions.Managers;

namespace RTFunctions.Functions
{
    public class ShapeUI
    {
        static string ShapesPath => FunctionsPlugin.BepInExAssetsPath + "Shapes/";

        static Dictionary<string, ShapeUI> dictionary;
        public static Dictionary<string, ShapeUI> Dictionary
        {
            get
            {
                if (dictionary == null)
                {
                    dictionary = new Dictionary<string, ShapeUI>();

                    // Square
                    {
                        dictionary.Add("rectangle", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_square.png", 0, 0));
                        dictionary.Add("rectangle_outline", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_square_outline.png", 0, 1));
                        dictionary.Add("rectangle_outline_thin", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_square_outline_thin.png", 0, 2));
                        dictionary.Add("diamond", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_diamond.png", 0, 3));
                        dictionary.Add("diamond_outline", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_diamond_outline.png", 0, 4));
                        dictionary.Add("diamond_outline_thin", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_diamond_outline_thin.png", 0, 5));
                    }

                    // Circle
                    {
                        dictionary.Add("circle", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_circle.png", 1, 0));
                        dictionary.Add("circle_outline", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_circle_outline.png", 1, 1));
                        dictionary.Add("half_circle", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_half_circle.png", 1, 2));
                        dictionary.Add("half_circle_outline", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_half_circle_outline.png", 1, 2));
                        dictionary.Add("circle_outline_thin", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_circle_outline_thin.png", 1, 3));
                        dictionary.Add("quarter_circle", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_quarter_circle.png", 1, 4));
                        dictionary.Add("quarter_circle_outline", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_quarter_circle_outline.png", 1, 5));
                        dictionary.Add("eighth_circle", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_eighth_circle.png", 1, 6));
                        dictionary.Add("eighth_circle_outline", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_eighth_circle_outline.png", 1, 7));
                        dictionary.Add("circle_outline_thinner", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_circle_outline_thinner.png", 1, 8));
                        dictionary.Add("quarter_circle_outline_thin", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_quarter_circle_outline_thin.png", 1, 9));
                        dictionary.Add("quarter_circle_outline_thinner", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_quarter_circle_outline_thinner.png", 1, 10));
                        dictionary.Add("half_circle_outline_thin", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_half_circle_outline_thin.png", 1, 11));
                        dictionary.Add("half_circle_outline_thinner", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_half_circle_outline_thinner.png", 1, 12));
                        dictionary.Add("eighth_circle_outline_thin", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_eighth_circle_outline_thin.png", 1, 13));
                        dictionary.Add("eighth_circle_outline_thinner", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_eighth_circle_outline_thinner.png", 1, 14));
                    }

                    // Triangle
                    {
                        dictionary.Add("triangle", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_triangle.png", 2, 0));
                        dictionary.Add("triangle_outline", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_triangle_outline.png", 2, 1));
                        dictionary.Add("triangle_outline_thin", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_triangle_outline_thin.png", 2, 2));
                        dictionary.Add("right_triangle", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_triangle.png", 2, 3));
                        dictionary.Add("right_triangle_outline", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_right_triangle_outline.png", 2, 4));
                    }

                    // Arrow
                    {
                        dictionary.Add("full_arrow", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_full_arrow.png", 3, 0));
                        dictionary.Add("top_arrow", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_top_arrow.png", 3, 1));
                        dictionary.Add("chevron_arrow", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_chevron_arrow.png", 3, 2));
                    }

                    // Text
                    {
                        dictionary.Add("text", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_text.png", 4, 0));
                    }

                    // Hexagon
                    {
                        dictionary.Add("hexagon", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_hexagon.png", 5, 0));
                        dictionary.Add("hexagon_outline", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_hexagon_outline.png", 5, 1));
                        dictionary.Add("hexagon_outline_thin", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_hexagon_outline_thin.png", 5, 2));
                        dictionary.Add("half_hexagon", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_half_hexagon.png", 5, 3));
                        dictionary.Add("half_hexagon_outline", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_half_hexagon_outline.png", 5, 4));
                        dictionary.Add("half_hexagon_outline_thin", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_half_hexagon_outline_thin.png", 5, 5));
                    }

                    // Image
                    {
                        dictionary.Add("image", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_image_obj.png", 6, 0));
                    }

                    // Pentagon
                    {
                        dictionary.Add("pentagon", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_hexagon.png", 7, 0));
                        dictionary.Add("hexagon_outline", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_hexagon_outline.png", 7, 1));
                        dictionary.Add("hexagon_outline_thin", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_hexagon_outline_thin.png", 7, 2));
                        dictionary.Add("half_hexagon", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_half_hexagon.png", 7, 3));
                        dictionary.Add("half_hexagon_outline", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_half_hexagon_outline.png", 7, 4));
                        dictionary.Add("half_hexagon_outline_thin", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_half_hexagon_outline_thin.png", 7, 5));
                    }

                    // Misc
                    {
                        dictionary.Add("pa_logo_top", new ShapeUI($"{RTFile.ApplicationDirectory}{ShapesPath}editor_gui_pa_logo_top.png", 8, 0));
                    }
                }

                return dictionary;
            }
            set => dictionary = value;
        }

        public ShapeUI(string shapePath, int shape, int shapeOption)
        {
            this.shapePath = shapePath;
            this.shape = shape;
            this.shapeOption = shapeOption;
        }

        public string shapePath;
        Sprite sprite;
        public Sprite Sprite
        {
            get
            {
                if (!sprite)
                    sprite = RTSpriteManager.GetSprite(shapePath, TextureFormat.Alpha8);

                return sprite;
            }
        }

        public int shape;
        public int shapeOption;
    }
}
