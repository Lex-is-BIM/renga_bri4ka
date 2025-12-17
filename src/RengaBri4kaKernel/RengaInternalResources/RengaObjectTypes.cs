using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RengaBri4kaKernel.RengaInternalResources
{
    internal class RengaObjectTypes
    {
        public static Guid AngularDimension = new Guid("{96788994-b7fc-41d7-8a99-d674543e9237}");
        public static Guid AssemblyInstance = new Guid("{00799249-1824-4ebd-bf93-40bb92efa9e6}");
        public static Guid Axis = new Guid("{4b41ccf8-c969-4c55-a1f2-cced9c164f07}");
        public static Guid Beam = new Guid("{63478188-7c88-4a6d-b891-9725f04a5bc7}");
        public static Guid Column = new Guid("{d9ee2442-e807-42fb-8fe5-9dcfe543035d}");
        public static Guid DiametralDimension = new Guid("{2aabe3a4-a29e-4534-a9f5-0f070fee240c}");
        public static Guid Door = new Guid("{1cfba99c-01e7-4078-ae1a-3e2ff0673599}");
        public static Guid DrawingText = new Guid("{688cce66-411f-44a2-a5cc-149bdde3169c}");
        public static Guid Duct = new Guid("{06cc88ee-9a67-4626-9c34-dde03c331a74}");
        public static Guid DuctAccessory = new Guid("{47d0d93f-3c7b-4269-bf8a-de246e1724d0}");
        public static Guid DuctFitting = new Guid("{77ffca60-b20e-49f0-b42f-4fdc9b1c825b}");
        public static Guid ElectricDistributionBoard = new Guid("{96da9155-43c1-42b8-bba2-b4f61fa43acc}");
        public static Guid Element = new Guid("{e1e3bd66-2e13-4fa4-a9eb-677e03067c2f}");
        public static Guid Elevation = new Guid("{8a49a9a8-a401-4ab1-8038-92093503c97a}");
        public static Guid Equipment = new Guid("{5d2f3734-5a49-4504-90b1-0676f0f25da7}");
        public static Guid Floor = new Guid("{f5bd8bd8-39c1-47f8-8499-f673c580dfbe}");
        public static Guid Hatch = new Guid("{84b43087-d4a4-4cce-b34d-40e283d9e691}");
        public static Guid Hole = new Guid("{ecef8f90-0cf9-4494-98de-91242a2a9f5c}");
        public static Guid IfcObject = new Guid("{f914251d-d5fa-48b2-b93b-074f442cbf3b}");
        public static Guid IsolatedFoundation = new Guid("{6063816c-89ff-4c8f-a814-3be6cb94128e}");
        public static Guid Level = new Guid("{c3ce17ff-6f28-411f-b18d-74fe957b2ba8}");
        public static Guid LightingFixture = new Guid("{793d3f7c-905d-4d85-a351-b152241dd2e7}");
        public static Guid Line3D = new Guid("{02bbebe8-e28b-4ee5-8916-11b514a35dca}");
        public static Guid LinearDimension = new Guid("{dc82ca1a-a0c3-4a1a-aefb-a7d720dd3a09}");
        public static Guid LineElectricalCircuit = new Guid("{83de45e6-4793-49ec-8b9e-65a2438f36de}");
        public static Guid LinkedDrawing = new Guid("{f6647dc9-cfae-4c6b-9312-cd6d8010c340}");
        public static Guid LinkedImage = new Guid("{857a042d-7d3c-4715-9ebf-95e2e9648adf}");
        public static Guid LinkedModel = new Guid("{67a0b42c-8c1e-47e8-b46e-78d8bb260de0}");
        public static Guid MechanicalEquipment = new Guid("{de4420ce-02b6-4b12-9cd7-9322118be8fe}");
        public static Guid ModelText = new Guid("{da557027-f243-4331-bb5b-853abc437cd7}");
        public static Guid Opening = new Guid("{fc443d5a-b76c-45e5-b91c-520ef0896109}");
        public static Guid Pipe = new Guid("{838cc9f6-e3d8-4132-af6f-c58df0f8d037}");
        public static Guid PipeAccessory = new Guid("{41e2788a-49ed-487f-9ae1-55b6e09ae6e5}");
        public static Guid PipeFitting = new Guid("{d31dc2e3-808e-4987-8481-7f86665a07fc}");
        public static Guid Plate = new Guid("{62cf086e-5a39-4484-840c-ffa6a1c6e2b7}");
        public static Guid PlumbingFixture = new Guid("{b8c7155a-b462-4ff5-bc41-c9c17a9f48fa}");
        public static Guid RadialDimension = new Guid("{377c2fda-9411-43ac-a6c6-0e3b520be721}");
        public static Guid Railing = new Guid("{a1aca786-78a4-4015-b412-9150baad71a9}");
        public static Guid Ramp = new Guid("{debde004-afcc-4da8-8dd0-4223ff836acd}");
        public static Guid Rebar = new Guid("{9fabc932-590f-4068-89a8-ee6ee3d7cbbf}");
        public static Guid Roof = new Guid("{bac4470f-d560-4f57-a49e-faa5f6e5a279}");
        public static Guid Room = new Guid("{f1a805ff-573d-f46b-ffba-57f4bccaa6ed}");
        public static Guid Route = new Guid("{8b323bee-3882-4744-8838-24f45df714a9}");
        public static Guid RoutePoint = new Guid("{ce93e320-7167-4cd1-92a8-5e42d546066b}");
        public static Guid Section = new Guid("{4166fd59-64c0-45ee-ae3b-49fae1257ef1}");
        public static Guid Stair = new Guid("{3f522f49-aee2-4d73-9866-9b07cf336a69}");
        [Obsolete]
        public static Guid TextObject = new Guid("{da557027-f243-4331-bb5b-853abc437cd7}");
        public static Guid Undefined = new Guid("{97675473-ca62-4ea4-bc6e-bb2ca57b7e67}");
        public static Guid Wall = new Guid("{4329112a-6b65-48d9-9da8-abf1f8f36327}");
        public static Guid WallFoundation = new Guid("{d7dd0293-dd65-4229-a64c-8b528d4e226f}");
        public static Guid Window = new Guid("{2b02b353-2ca5-4566-88bb-917ea8460174}");
        public static Guid WiringAccessory = new Guid("{b00d5c25-92a8-4409-a3b7-7c37ed792c06}");

        public static List<Tuple<Guid, string>> GetAll()
        {
            List<Tuple<Guid, string>> res = new List<Tuple<Guid, string>>();
            var t = Guid.Empty;
            RengaObjectTypes rO = new RengaObjectTypes();
            var fields = typeof(RengaObjectTypes).GetFields();
            foreach (var field in fields)
            {
                res.Add(Tuple.Create<Guid, string>((Guid)field.GetValue(rO), field.Name));
            }
            return res;
        }
    }
}
