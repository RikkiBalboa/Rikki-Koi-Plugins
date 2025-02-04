using System;
using System.Collections.Generic;
using System.Text;

namespace Plugins
{
    public class UIMappings
    {
        public static readonly Dictionary<string, int> ClothingKinds = new Dictionary<string, int>
        {
            { "Top", 0 },
            { "Bottom", 1 },
            { "Bra", 2 },
            { "Underwear", 3 },
            { "Gloves", 4 },
            { "Pantyhose", 5 },
            { "Legwear", 6 },
#if KK
            { "Shoes (Indoors)", 7 },
            { "Shoes (Outdoors)", 8 }
#elif KKS
            { "Shoes", 8 }
#endif
        };

        public static readonly Dictionary<SubCategory, Dictionary<int, string>> ShapeBodyValueMap = new Dictionary<SubCategory, Dictionary<int, string>>()
        {
            {
                SubCategory.BodyGeneral, new Dictionary<int, string>()
                {
                    {0,  "Body Height"},
                    {1,  "Head Size"},
                }
            },
            {
                SubCategory.BodyChest, new Dictionary<int, string>()
                {
                    {4, "Breast Size"},
                    {5, "Breast Vertical Position"},
                    {6, "Breast Spacing"},
                    {7, "Breast Horizontal Position"},
                    {8, "Breast Vertical Angle"},
                    {9, "Breast Depth"},
                    {10, "Breast Roundess"},
                    {11, "Areola Depth"},
                    {12, "Nipple Thickness"},
                    {13, "Nipple Depth"},
                }
            },
            {
                SubCategory.BodyUpper, new Dictionary<int, string>()
                {
                    {2, "Neck Width"},
                    {3, "Neck Thickness"},
                    {14, "Shoulder Width"},
                    {15, "Shoulder Thickness"},
                    {16, "Upper Torso Width"},
                    {17, "Upper Torso Thickness"},
                    {18, "Lower Torso Width"},
                    {19, "Lower Torso Thickness"},
                }
            },
            {
                SubCategory.BodyLower, new Dictionary<int, string>()
                {
                    {20, "Waist Position"},
                    {21, "Belly Thickness"},
                    {22, "Waist Width"},
                    {23, "Waist Thickness"},
                    {24, "Hip Width"},
                    {25, "Hip Thickness"},
                    {26, "Butt Size"},
                    {27, "Butt Angle"},
                }
            },
            {
                SubCategory.BodyArms, new Dictionary<int, string>()
                {
                    {37, "Shoulder Width"},
                    {38, "Shoulder Thickness"},
                    {39, "Upper Arm Width"},
                    {40, "Upper Arm Thickness"},
                    {41, "Elbow Width"},
                    {42, "Elbow Thickness"},
                    {43, "Forearm Thickness"},
                }
            },
            {
                SubCategory.BodyLegs, new Dictionary<int, string>()
                {
                    {28, "Upper Thigh Width"},
                    {29, "Upper Thigh Thickness"},
                    {30, "Lower Thigh Width"},
                    {31, "Lower Thigh Thickness"},
                    {32, "Knee Width"},
                    {33, "Knee Thickness"},
                    {34, "Calves"},
                    {35, "Ankle Width"},
                    {36, "Ankle Thickness"},
                }
            },
        };
        public static readonly Dictionary<SubCategory, Dictionary<int, string>> ShapeFaceValueMap = new Dictionary<SubCategory, Dictionary<int, string>>()
        {
            {
                SubCategory.FaceGeneral, new Dictionary<int, string>()
                {
                    {0, "Face Width"},
                    {1, "Upper Face Depth"},
                    {2, "Upper Face Height"},
                    {3, "Upper Face Size"},
                    {4, "Lower Face Depth"},
                    {5, "Lower Face Width"},
                }
            },
            {
                SubCategory.FaceEars, new Dictionary<int, string>()
                {
                    {47, "Ear Size"},
                    {48, "Ear Angle Y Axis"},
                    {49, "Ear Angle Z Axis"},
                    {50, "Upper Ear Shape"},
                    {51, "Lower Ear Shape"},
                }
            },
            {
                SubCategory.FaceJaw, new Dictionary<int, string>()
                {
                    {6, "Lower Jaw Vertical Position"},
                    {7, "Lower Jaw Depth"},
                    {8, "Jaw Vertical Position"},
                    {9, "Jaw Width"},
                    {10, "Jaw Depth"},
                    {11, "Chin Vertical Position"},
                    {12, "Chin Depth"},
                    {13, "Chin Width"},
                }
            },
            {
                SubCategory.FaceCheeks, new Dictionary<int, string>()
                {
                    {14, "Cheekbone Width"},
                    {15, "Cheekbone Depth"},
                    {16, "Cheek Width"},
                    {17, "Cheek Depth"},
                    {18, "Cheek Vertical Position"},
                }
            },
            {
                SubCategory.FaceEyebrows, new Dictionary<int, string>()
                {
                    {19, "Eyebrow Vertical Position"},
                    {20, "Eyebrow Spacing"},
                    {21, "Eyebrow Angle"},
                    {22, "Inner Eyebrow Shape"},
                    {23, "Outer Eyebrow Shape"},
                }
            },
            {
                SubCategory.FaceEyes, new Dictionary<int, string>()
                {
                    {24, "Upper Eyelid Shape 1"},
                    {25, "Upper Eyelid Shape 2"},
                    {26, "Upper Eyelid Shape 3"},
                    {27, "Lower Eyelid Shape 1"},
                    {28, "Lower Eyelid Shape 2"},
                    {29, "Lower Eyelid Shape 3"},
                    {30, "Eye Vertical Position"},
                    {31, "Eye Spacing"},
                    {32, "Eye Depth"},
                    {33, "Eye Rotation"},
                    {34, "Eye Height"},
                    {35, "Eye Width"},
                    {36, "Inner Eye Corner Height"},
                    {37, "Outer Eye Corner Height"},
                }
            },
            {
                SubCategory.FaceNose, new Dictionary<int, string>()
                {
                    {38, "Nose Tip Height"},
                    {39, "Nose Vertical Position"},
                    {40, "Nose Ridge Height"},
                }
            },
            {
                SubCategory.FaceMouth, new Dictionary<int, string>()
                {
                    {41, "Mouse Vertical Position"},
                    {42, "Mouth Width"},
                    {43, "Mouth Depth"},
                    {44, "Upper Lip Depth"},
                    {45, "Lower Lip Depth"},
                    {46, "Mouth Corner Shape"},
                }
            },
        };

        public static string GetSubcategoryName(SubCategory subcategory)
        {
            switch (subcategory)
            {
                case SubCategory.BodyGeneral: return "General";
                case SubCategory.BodyChest: return "Chest";
                case SubCategory.BodyUpper: return "Upper Body";
                case SubCategory.BodyLower: return "Lower Body";
                case SubCategory.BodyArms: return "Arms";
                case SubCategory.BodyLegs: return "Legs";
                case SubCategory.BodyPubicHair: return "Pubic Hair";
                case SubCategory.BodySuntan:return "Suntan";
                case SubCategory.FaceGeneral: return "General";
                case SubCategory.FaceEars: return "Ears";
                case SubCategory.FaceJaw: return "Jaw";
                case SubCategory.FaceCheeks: return "Cheeks";
                case SubCategory.FaceEyebrows: return "Eyebrows";
                case SubCategory.FaceEyes: return "Eyes";
                case SubCategory.FaceIris: return "Iris";
                case SubCategory.FaceNose: return "Nose";
                case SubCategory.FaceMouth: return "Mouth";
                case SubCategory.FaceMakeup: return "Makeup";
                case SubCategory.ClothingTop: return "Top";
                case SubCategory.ClothingBottom: return "Bottom";
                case SubCategory.ClothingBra: return "Bra";
                case SubCategory.ClothingUnderwear: return "Underwear";
                case SubCategory.ClothingGloves: return "Gloves";
                case SubCategory.ClothingPantyhose: return "Pantyhose";
                case SubCategory.ClothingLegwear: return "Legwear";
#if KK
                case SubCategory.ClothingShoesInDoors: return "Shoes (Indoors)";
                case SubCategory.ClothingShoesOutdoors: return "Shoes (Outdoors";
#elif KKS
                case SubCategory.ClothingShoes: return "Shoes";
#endif
                default: return "Undefined";
            }
        }
    }

    public enum Category
    {
        Body,
        Face,
        Hair,
        Clothing,
        Accessories,
    }

    public enum SubCategory
    {
        BodyGeneral,
        BodyChest,
        BodyUpper,
        BodyLower,
        BodyArms,
        BodyLegs,
        BodyPubicHair,
        BodySuntan,
        FaceGeneral,
        FaceEars,
        FaceJaw,
        FaceCheeks,
        FaceEyebrows,
        FaceEyes,
        FaceIris,
        FaceNose,
        FaceMouth,
        FaceMakeup,
        ClothingTop,
        ClothingBottom,
        ClothingBra,
        ClothingUnderwear,
        ClothingGloves,
        ClothingPantyhose,
        ClothingLegwear,
#if KK
        ClothingShoesInDoors,
        ClothingShoesOutdoors,
#elif KKS
        ClothingShoes,
#endif
    }
}
