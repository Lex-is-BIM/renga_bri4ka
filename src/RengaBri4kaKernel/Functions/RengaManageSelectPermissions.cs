using Renga;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using RengaBri4kaKernel.Configs;
using RengaBri4kaKernel.AuxFunctions;
using RengaBri4kaKernel.RengaInternalResources;

namespace RengaBri4kaKernel.Functions
{
    //Реализация https://github.com/GeorgGrebenyuk/Renga_FollowUsersActions
    public class RengaManageSelectPermissions
    {
        private RengaManageSelectPermissions()
        {
            mActiveConfig = new RengaManageSelectPermissionsConfig();
            mEventSelection = new SelectionEventSource(PluginData.rengaApplication.Selection);
            mEventSelection.ModelSelectionChanged += on_selection;
        }

        public static RengaManageSelectPermissions CreateInstance()
        {
            if (mInstance == null) mInstance = new RengaManageSelectPermissions();
            return mInstance;
        }

        public void SetConfig(RengaManageSelectPermissionsConfig? config)
        {
            if (config == null) return;
            mActiveConfig = config;

            // Нужно зарегистрировать свойство и назначить его всем типам объектом. Также задать enum-значения

            //зарегистрировать параметры, если ониотсутствуют
            RengaPropertiesUtils.RegisterPropertyIfNotReg(FollowSelectionId, FollowSelection, PropertyType.PropertyType_Enumeration, config.PropertyValues.ToArray());
            RengaPropertiesUtils.AssignPropertiesToTypes(FollowSelectionId, null);

            var existedValues = RengaPropertiesUtils.GetEnumerationValues(FollowSelectionId);

            // Значение по умолчанию попадает в число значений свойств только при регистрации
            //if (existedValues.Contains(RengaManageSelectPermissionsConfig.NoBehaviourName) && !config.PropertyValues.Contains(RengaManageSelectPermissionsConfig.NoBehaviourName)) 
            //{
            //    config.PropertyValues = new List<string>() { RengaManageSelectPermissionsConfig.NoBehaviourName }.Concat(config.PropertyValues).ToList();
            //}

            // Если новые значения отличны от существующих, то нужно обновить определение свойства
            if (!Array.Equals(existedValues, config.PropertyValues))
            {
                //if (!config.PropertyValues.Contains(RengaManageSelectPermissionsConfig.NoBehaviourName)) config.PropertyValues = new List<string>() { RengaManageSelectPermissionsConfig.NoBehaviourName }.Concat(config.PropertyValues).ToList();

                Dictionary<string, string> propsMap = new Dictionary<string, string>();
                foreach (string? existedProp in existedValues)
                {
                    if (existedProp != null && existedProp.Length > 1) 
                    {
                        if (config.PropertyValues.Contains(existedProp)) propsMap.Add(existedProp, existedProp);
                        else propsMap.Add(existedProp, RengaManageSelectPermissionsConfig.NoBehaviourName);
                    }
                }

                RengaPropertiesUtils.SetEnumerationValues(FollowSelectionId, config.PropertyValues.ToArray(), propsMap);
            }    
        }

        public void on_selection(object sender, EventArgs args)
        {
            //if (!can_start_following) return;
            Renga.ISelection selection = PluginData.rengaApplication.Selection;
            List<int> selected_objects_id = selection.GetSelectedObjects().OfType<int>().ToList();

            /* При каждом новом выборе пользователем объектов модели действуют следующие операции:
                1. Получаются идентификаторы объектов модели, выделенных Пользователем;
                2. Если объекты модели не имеют тип Renga.EntityTypes.Undefined:
                    2.1. Если у модели заполнена свойство Enumeration и оно != '_NO':
                    2.2. Если это свойство не входит в набор свойств, разрешенных для пользователя по файлу сопоставления, то 
                этот идентификатор объекта фиксируется во временный список
                3. В зависимости от выбранной логики работы с выделенными объектами чужих типов, по форме в классе UsersSelection.cs
                аквтивируется соответствующий режим.
             */

            //Список для объектов, которые Пользователь не имеет права трогать
            List<int> wrong_objects = new List<int>();
            List<string> wrong_objects_names = new List<string>();
            Renga.IModelObjectCollection? model_objects = null;
            try
            {
                model_objects = PluginData.rengaApplication.Project.Model.GetObjects();

            }
            catch (Exception ex) { }

            if (model_objects == null) return;

            foreach (int internal_model_object_id in selected_objects_id)
            {
                Renga.IModelObject one_object = model_objects.GetById(internal_model_object_id);
                //Исключение от ошибок
                if (one_object.ObjectType != RengaEntityTypes.UndefinedObject)
                {
                    if (one_object.GetProperties() == null) continue;

                    Renga.IProperty? followProperty = one_object.GetProperties().GetS(FollowSelectionPropertyId);

                    if (followProperty != null && followProperty.HasValue() && followProperty.GetEnumerationValue() != RengaManageSelectPermissionsConfig.NoBehaviourName)
                    {
                        if (!mActiveConfig.AcceptedRoles.Contains(followProperty.GetEnumerationValue()))
                        {
                            wrong_objects.Add(internal_model_object_id);
                            wrong_objects_names.Add(one_object.Name);
                        }
                    }
                }
            }
            if (wrong_objects.Any())
            {
                List<int> empty_array = new List<int>();
                if (mActiveConfig.OnSelectMode == BehaviorOnSelectUnaccessObjects.SkipSelectionWithExceptions)
                {
                    PluginData.rengaApplication.Selection.SetSelectedObjects(empty_array.ToArray());
                    PluginData.rengaApplication.Selection.SetSelectedObjects(selected_objects_id.Except(wrong_objects).ToArray());
                    PluginData.rengaApplication.UI.ShowMessageBox(MessageIcon.MessageIcon_Info, "Сообщение", "Было снято выделение с объектов, которые вам запрещено изменять!");
                }
                else if (mActiveConfig.OnSelectMode == BehaviorOnSelectUnaccessObjects.SkipSelectionWithoutExceptions)
                {
                    PluginData.rengaApplication.Selection.SetSelectedObjects(empty_array.ToArray());
                    PluginData.rengaApplication.Selection.SetSelectedObjects(selected_objects_id.Except(wrong_objects).ToArray());
                }
                else if (mActiveConfig.OnSelectMode == BehaviorOnSelectUnaccessObjects.ShowSelectionDialog)
                {
                    int message_box_return_type = MessageBox(IntPtr.Zero, $"Среди выделенных объектов есть объекты ({wrong_objects.Count()} шт), \n " +
                        String.Join("\n", wrong_objects_names.ToArray()) +
                        "\n для выбора которых требуется настоящее уведомление. Вы уверены, что хотите продолжить? " +
                        "Если вы нажмете на 'Нет' - то с них выбор снимется. Если нажмете на 'Да' - то выбор сохранится", "Предупреждение",
                    MB_ICONQUESTION | MB_YESNO | MB_DEFBUTTON1);
                    if (message_box_return_type == IDNO)
                    {
                        PluginData.rengaApplication.Selection.SetSelectedObjects(empty_array.ToArray());
                        PluginData.rengaApplication.Selection.SetSelectedObjects(selected_objects_id.Except(wrong_objects).ToArray());
                    }
                    else
                    {
                        //nothing
                    }
                }
            }
        }

        private Renga.SelectionEventSource mEventSelection;

        internal RengaManageSelectPermissionsConfig mActiveConfig;

        //public int mode_selection = 1;
        //public bool can_start_following = false;

        public const string FollowSelectionPropertyId = "61a27b1d-60f3-4a70-94b8-5f72252021c9";
        public static Guid FollowSelectionId = new Guid(FollowSelectionPropertyId);
        public const string FollowSelection = "Bri4ka. Отслеживание пользовательского выбора";

        //public static Guid our_property_id = Guid.Parse("94f7fd69-2c9f-4c44-80b8-36524ab29a18");
        //public List<string> permitted_design_sections = null;
        private static RengaManageSelectPermissions? mInstance;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
        // A few MessageBox-related constants:
        public static uint MB_ICONQUESTION = 0x00000020;
        public static uint MB_YESNO = 0x00000004;
        public static uint MB_DEFBUTTON1 = 0x00000000;
        public static int IDNO = 7;

        ~RengaManageSelectPermissions()
        {
            mEventSelection.Dispose();
        }
    }
}
