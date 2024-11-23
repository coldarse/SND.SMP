using System;
namespace SND.SMP.Shared
{
	public class EnumConst
	{
        public class QueueEnumConst
        {
            public const string STATUS_NEW = "New";
            public const string STATUS_RUNNING = "Running";
            public const string STATUS_FINISH = "Finish";
            public const string STATUS_ERROR = "Error";
            public const string STATUS_UPDATING = "Updating";

            public const string EVENT_TYPE_DISPATCH_VALIDATE = "Validate Dispatch";
            public const string EVENT_TYPE_DISPATCH_UPLOAD = "Upload Dispatch";
            public const string EVENT_TYPE_RATE_WEIGHT_BREAK = "Rate Weight Break";
            public const string EVENT_TYPE_API_ITEM_ID_ADD = "Add API Item ID";
            public const string EVENT_TYPE_POOL_ID_GEN = "Generate Pool Item ID";

            public const string EVENT_TYPE_TRACKING_UPLOAD = "Upload Tracking";

            public const string EVENT_TYPE_TRACKING_UPDATE = "Update Tracking";
            public const string EVENT_TYPE_TRACKING_STATUS = "Tracking Status";

            public const string EVENT_TYPE_DISPATCH_TRACKING_UPDATE = "Update Dispatch Tracking";




        }

        public class TrackingNoForUpdateConst
        {
            public const string STATUS_UPDATE = "UPDATE";
            public const string STATUS_DELETE = "DELETE";
        }

        public class DispatchEnumConst
        {
            public const string SERVICE_TS = "TS";
            public const string SERVICE_DE = "DE";

            public const string PRODUCT_OMT = "OMT";
            public const string PRODUCT_R = "R";
            public const string PRODUCT_PRT = "PRT";

            public const string TV = "TV";
            public const string GQ = "GQ";
            public const string TH = "TH";
            public const string CO = "CO";
            public const string CL = "CL";
            public const string MY = "MY";
            public const string KG = "KG";
            public const string GE = "GE";
            public const string SL = "SL";
            public const string SA = "SA";

            public enum Status
            {
                Stage1 = 1,
                Stage2 = 2,
                Stage3 = 3,
                Stage4 = 4,
                Stage5 = 5,
                Stage6 = 6
            }

            public enum ImportFileType
            {
                Excel
            }
        }

        public class DispatchValidationEnumConst
        {
            public const string STATUS_RUNNING = "Running";
            public const string STATUS_FINISH = "Finish";
        }

        public class GenerateConst
        {
            public const string Status_Generating = "Generating";
            public const string Status_Approved = "Approved";
            public const string Status_Declined = "Declined";
            public const string Status_Completed = "Approved & Generated";
        }


        public class GlobalConst
        {
            public const string Albums = "Albums";
        }
	}
}

