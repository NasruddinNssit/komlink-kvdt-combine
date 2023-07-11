﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Menu.Section
{
	public interface IMenuItemSection
	{
		bool IsEditMode { get; }
		bool IsEditAllowed { get; set; }
		void OpenEdit();
	}
}
