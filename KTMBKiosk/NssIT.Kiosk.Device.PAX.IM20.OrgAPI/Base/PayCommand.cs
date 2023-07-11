using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.PAX.IM20.OrgAPI.Base
{
	public struct PayCommand
	{
		/* Basic/Init */
		public const string Base_PreAuth = "C100";
		public const string Base_ReadCard = "C910";
		public const string Base_Echo = "C902";

		/* Normal ECR */
		public const string Norm_Adjust = "C220";
		public const string Norm_CashBack = "C205";
		public const string Norm_ReFund = "C203";
		public const string Norm_Sale = "C200";
		public const string Norm_Settlement = "C500";
		public const string Norm_Void = "C201";
		public const string Norm_Query = "C208";
		

		/* Normal AliPay */
		public const string Ali_ReFund = "C292";
		public const string Ali_Sale = "C290";
		public const string Ali_Void = "C291";

		/* ORS */
		public const string Ors_PointEnq = "C241";

		public const string Ors_GiftRed = "C245";
		public const string Ors_HotDeal = "C246";
		public const string Ors_InstReward = "C242";
		public const string Ors_PointRed = "C243";
		public const string Ors_ValueRed = "C244";

		/* ORS Void for GiftReward, HotDeal, InstantReward, PointReward & ValueReward */
		public const string Ors_Void = "C342";

	}
}
