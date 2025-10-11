using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RengaBri4kaKernel.Geometry;

namespace RengaBri4kaKernel.Configs
{
    /// <summary>
    /// Вспомогательный класс для описания геометрии объекта, его определяющих свойств и матрицы трансформации для положения
    /// </summary>
    public class FacetedGeometryObjectConfig : ConfigIO
    {
        public double[] Matrix4x4 { get; set; }
        public Guid ObjectCategoryType { get; set; }
        public List<ObjectParameter> Parameters { get; set; }
        public List<FacetedBRepSolid> Geometry { get; set; }

        public Guid ObjectId { get; set; }

        public FacetedGeometryObjectConfig()
        {
            Matrix4x4 = new double[16];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    int index = i * 4 + j;
                    Matrix4x4[index] = 0;
                    if (i == j) Matrix4x4[index] = 1;

                }
            }
            Parameters = new List<ObjectParameter>();
            Geometry = new List<FacetedBRepSolid>();
            ObjectCategoryType = Guid.Empty;

        }
    }

    public class ObjectParameter
    {
        public Guid Id { get; set; }
        public double Value { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            ObjectParameter? objOther = obj as ObjectParameter;
            if (objOther == null) return false;
            if (objOther.Id.Equals(Id) && objOther.Value.Equals(Value)) return true;
            return false;
        }
    }

}
