using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace DT1.Watchdog.Command
{
    interface INavigationAccess
    {
		INavigation Navigation { get; }
	}
}
