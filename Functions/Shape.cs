using UnityEngine;

using RTFunctions.Functions.Managers;

namespace RTFunctions.Functions
{
    public struct Shape
    {
        public Shape(string name, int type, int option, bool includeTextImage = true)
        {
            this.name = name;
            this.type = type;
            this.option = option;
            this.includeTextImage = includeTextImage;
            mesh = null;
        }

        public string name;

        public int type;
        public int option;

        public bool includeTextImage;

        public Mesh mesh;

        public int Type
        {
            get => Mathf.Clamp(type, 0, HasCustomShapes ? includeTextImage ? maxShapes.Length - 1 : maxShapesTI.Length - 1 : includeTextImage ? maxShapesDefault.Length - 1 : maxShapesDefaultTI.Length - 1);
            set => type = Mathf.Clamp(value, 0, HasCustomShapes ? includeTextImage ? maxShapes.Length - 1 : maxShapesTI.Length - 1 : includeTextImage ? maxShapesDefault.Length - 1 : maxShapesDefaultTI.Length - 1);
        }

        public int Option
        {
            get => Mathf.Clamp(option, 0, HasCustomShapes ? includeTextImage ? maxShapes[Type] : maxShapesTI[Type] : includeTextImage ? maxShapesDefault[Type] : maxShapesDefaultTI[Type]);
            set => option = Mathf.Clamp(value, 0, HasCustomShapes ? includeTextImage ? maxShapes[Type] : maxShapesTI[Type] : includeTextImage ? maxShapesDefault[Type] : maxShapesDefaultTI[Type]);
        }

        public int this[int index]
        {
            get
            {
                int result;
                switch (index)
                {
                    case 0:
                        {
                            result = Type;
                            break;
                        }
                    case 1:
                        {
                            result = Option;
                            break;
                        }
                    default:
                        throw new System.IndexOutOfRangeException("Invalid Shape index!");
                }
                return result;
            }
            set
            {
                switch (index)
                {
                    case 0:
                        {
                            Type = value;
                            break;
                        }
                    case 1:
                        {
                            Option = value;
                            break;
                        }
                    default:
                        throw new System.IndexOutOfRangeException("Invalid Shape index!");
                }
            }
        }

        #region Methods

        public static Shape DeepCopy(Shape orig) => new Shape
        {
            name = orig.name,
            type = orig.type,
            option = orig.option,
            includeTextImage = orig.includeTextImage,
            mesh = orig.mesh
        };

        public void Clamp()
        {
            if (includeTextImage)
            {
                type = Mathf.Clamp(type, 0, HasCustomShapes ? maxShapes.Length - 1 : maxShapesDefault.Length - 1);
                option = Mathf.Clamp(option, 0, HasCustomShapes ? maxShapes[type] : maxShapesDefault[type]);
            }
            else
            {
                type = Mathf.Clamp(type, 0, HasCustomShapes ? maxShapesTI.Length - 1 : maxShapesDefaultTI.Length - 1);
                option = Mathf.Clamp(option, 0, HasCustomShapes ? maxShapesTI[type] : maxShapesDefaultTI[type]);
            }
        }

        #endregion

        #region Operators

        public static bool operator ==(Shape a, Shape b) => a.type == b.type && a.option == b.option;
        public static bool operator !=(Shape a, Shape b) => a.type != b.type || a.option != b.option;

        public static bool operator >(Shape a, Shape b) => a.type > b.type && a.option > b.option;
        public static bool operator <(Shape a, Shape b) => a.type < b.type && a.option < b.option;
        public static bool operator >=(Shape a, Shape b) => a.type >= b.type && a.option >= b.option;
        public static bool operator <=(Shape a, Shape b) => a.type <= b.type && a.option <= b.option;

        public static implicit operator bool(Shape exists) => exists != null;

        public override bool Equals(object obj)
        {
            if (obj is Shape)
                return this == (Shape)obj;
            return false;
        }

        public override int GetHashCode() => type.GetHashCode() ^ option.GetHashCode();

        public override string ToString() => $"{name}: ({type}, {option})";

        #endregion

        #region Global Properties

        public static bool HasCustomShapes => ModCompatibility.shapesPlugin != null && ModCompatibility.shapesPluginInstance != null;

        public int[] MaxShapes
        {
            get
            {
                if (HasCustomShapes && includeTextImage)
                    return maxShapes;
                else if (HasCustomShapes)
                    return maxShapesTI;
                else if (includeTextImage)
                    return maxShapesDefault;
                else
                    return maxShapesDefaultTI;
            }
        }

        static int[] maxShapesDefault = new int[]
        {
            2,
            8,
            3,
            1,
            0,
            5
        };
        
        static int[] maxShapesDefaultTI = new int[]
        {
            2,
            8,
            3,
            1,
            5
        };

        static int[] maxShapes = new int[]
        {
            5,
            15,
            4,
            2,
            0,
            5,
            0,
            5,
            22
        };

        static int[] maxShapesTI = new int[]
        {
            5,
            15,
            4,
            2,
            5,
            5,
            22
        };

        #endregion
    }
}
