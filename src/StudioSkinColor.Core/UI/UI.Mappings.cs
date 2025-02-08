using System;
using System.Collections.Generic;
using System.Text;
using static ChaCustom.CustomSelectKind;

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
                case SubCategory.HairBack: return "Back";
                case SubCategory.HairFront: return "Front";
                case SubCategory.HairSide: return "Side";
                case SubCategory.HairExtensions: return "Extensions";
                case SubCategory.HairMiscellaneous: return "Miscellaneous";
#endif
                default: return "Undefined";
            }
        }

        public static string GetSelectKindTypeName(SelectKindType type)
        {
            switch (type)
            {
                case SelectKindType.FaceDetail:
                    return "Face Overlay Type";
                case SelectKindType.Eyebrow:
                    return "Eyebrow Type";
                case SelectKindType.EyelineUp:
                    return "Upper eyeliner Type";
                case SelectKindType.EyelineDown:
                    return "Lower Eyeliner Type";
                case SelectKindType.EyeWGrade:
                    return "Sclera Type";
                case SelectKindType.EyeHLUp:
                    return "Upper Highlight Type";
                case SelectKindType.EyeHLDown:
                    return "Lower Highlight Type";
                case SelectKindType.Pupil:
                    return "Eye Type";
                case SelectKindType.PupilGrade:
                    return "Eye Gradient Type";
                case SelectKindType.Nose:
                    return "Nose Type";
                case SelectKindType.Lipline:
                    return "Lip Line Type";
                case SelectKindType.Mole:
                    return "Mole Type";
                case SelectKindType.Eyeshadow:
                    return "Eyeshadow Type";
                case SelectKindType.Cheek:
                    return "Cheek Type";
                case SelectKindType.Lip:
                    return "Lip Type";
                case SelectKindType.FacePaint01:
                    return "Paint 01 Type";
                case SelectKindType.FacePaint02:
                    return "Paint 02 Type";
                case SelectKindType.BodyDetail:
                    return "Skin Type";
                case SelectKindType.Nip:
                    return "Nipple Type";
                case SelectKindType.Underhair:
                    return "Pubic Hair Type";
                case SelectKindType.Sunburn:
                    return "Suntan Type";
                case SelectKindType.BodyPaint01:
                    return "Paint 01 Type";
                case SelectKindType.BodyPaint02:
                    return "Paint 02 Type";
                case SelectKindType.BodyPaint01Layout:
                    return "Paint 01 Position";
                case SelectKindType.BodyPaint02Layout:
                    return "Paint 02 Position";
                case SelectKindType.HairBack:
                    return "Back Hair Type";
                case SelectKindType.HairFront:
                    return "Front Hair Type";
                case SelectKindType.HairSide:
                    return "Side Hair Type";
                case SelectKindType.HairExtension:
                    return "Extension Type";
                case SelectKindType.CosTop:
                    return "Top Type";
                case SelectKindType.CosSailor01:
                    return "Body Type";
                case SelectKindType.CosSailor02:
                    return "Collar Type";
                case SelectKindType.CosSailor03:
                    return "Decoration Type";
                case SelectKindType.CosJacket01:
                    return "Innerwear Type";
                case SelectKindType.CosJacket02:
                    return "Outerwear Type";
                case SelectKindType.CosJacket03:
                    return "Decoration Type";
                case SelectKindType.CosTopPtn01:
                case SelectKindType.CosBotPtn01:
                case SelectKindType.CosBraPtn01:
                case SelectKindType.CosShortsPtn01:
                case SelectKindType.CosGlovesPtn01:
                case SelectKindType.CosPanstPtn01:
                case SelectKindType.CosSocksPtn01:
                case SelectKindType.CosInnerShoesPtn01:
                case SelectKindType.CosOuterShoesPtn01:
                    return "Cloth Pattern ①";
                case SelectKindType.CosTopPtn02:
                case SelectKindType.CosBotPtn02:
                case SelectKindType.CosBraPtn02:
                case SelectKindType.CosShortsPtn02:
                case SelectKindType.CosGlovesPtn02:
                case SelectKindType.CosPanstPtn02:
                case SelectKindType.CosSocksPtn02:
                case SelectKindType.CosInnerShoesPtn02:
                case SelectKindType.CosOuterShoesPtn02:
                    return "Cloth Pattern ②";
                case SelectKindType.CosTopPtn03:
                case SelectKindType.CosBotPtn03:
                case SelectKindType.CosBraPtn03:
                case SelectKindType.CosShortsPtn03:
                case SelectKindType.CosGlovesPtn03:
                case SelectKindType.CosPanstPtn03:
                case SelectKindType.CosSocksPtn03:
                case SelectKindType.CosInnerShoesPtn03:
                case SelectKindType.CosOuterShoesPtn03:
                    return "Cloth Pattern ③";
                case SelectKindType.CosTopPtn04:
                case SelectKindType.CosBotPtn04:
                case SelectKindType.CosBraPtn04:
                case SelectKindType.CosShortsPtn04:
                case SelectKindType.CosGlovesPtn04:
                case SelectKindType.CosPanstPtn04:
                case SelectKindType.CosSocksPtn04:
                case SelectKindType.CosInnerShoesPtn04:
                case SelectKindType.CosOuterShoesPtn04:
                    return "Cloth Pattern ④";
                case SelectKindType.CosTopEmblem:
                case SelectKindType.CosBotEmblem:
                case SelectKindType.CosBraEmblem:
                case SelectKindType.CosShortsEmblem:
                case SelectKindType.CosGlovesEmblem:
                case SelectKindType.CosPanstEmblem:
                case SelectKindType.CosSocksEmblem:
                case SelectKindType.CosInnerShoesEmblem:
                case SelectKindType.CosOuterShoesEmblem:
                    return "Emblem 02 Type";
                case SelectKindType.CosBot:
                    return "Bottom Type";
                case SelectKindType.CosBra:
                    return "Bra Type";
                case SelectKindType.CosShorts:
                    return "Underwear Type";
                case SelectKindType.CosGloves:
                    return "Gloves Type";
                case SelectKindType.CosPanst:
                    return "Pantyhose Type";
                case SelectKindType.CosSocks:
                    return "Legwear Type";
                case SelectKindType.CosInnerShoes:
                    return "Inner Shoe Type";
                case SelectKindType.CosOuterShoes:
#if KK
                    return "Outer Shoe Type";
#elif KKS
                    return "Shoe Type";
#endif
                case SelectKindType.HairGloss:
                    return "Hihglight Type";
                case SelectKindType.HeadType:
                    return "Face Type";
                case SelectKindType.CosTopEmblem2:
                case SelectKindType.CosBotEmblem2:
                case SelectKindType.CosBraEmblem2:
                case SelectKindType.CosShortsEmblem2:
                case SelectKindType.CosGlovesEmblem2:
                case SelectKindType.CosPanstEmblem2:
                case SelectKindType.CosSocksEmblem2:
                case SelectKindType.CosInnerShoesEmblem2:
                case SelectKindType.CosOuterShoesEmblem2:
                    return "Emblem 02 Type";
            }
            return "Undefined";
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
        HairBack,
        HairFront,
        HairSide,
        HairExtensions,
        HairMiscellaneous,
    }
}
