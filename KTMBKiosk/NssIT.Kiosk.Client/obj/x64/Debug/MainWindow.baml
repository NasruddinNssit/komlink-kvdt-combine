   M S B A M L   `   `   `  ����M  INssIT.Kiosk.Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:NssIT.Kiosk.ClientNssIT.Kiosk.Client  5   	   #    NssIT.Kiosk.Client.MainWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   2local clr-namespace:NssIT.Kiosk.Client     5   	   -   5          �� IsPressAndHoldEnabled  .  ��.  � WindowStartupLocation$ CenterScreen=�5
   	    � Title$ Kiosk Ticketing��5   	    � WindowStyle$
 None=�6!   .��5      �� $��Hidden=�6   $��Roboto)�68   ��.  �� PanningMode$
 Both=�6d   .��5      �� $
��1080��6   $
��1920��6   ._�5      �  � ColumnDefinitions 5      �� 5      -   5         ��Name$	 C00��5   '   $��* �64    � RowDefinitions 5      �� 5      -   5         ��Name$	 R00��5   $   $
��Auto �61   �� 5      -   5        $	 R02��5   $   $��* �61   .+�5#       � -   5        	  �Name$	 frmWorkTicketSummary��5#      $��0��66   $��0��6F   
�������
  � NavigationUIVisibility$
 Hidden=�6f    � 5%      -   5        $	 frmWorkDetail��5%      $��0��6/   $��1��6?   
���������. $
 Hidden=�6�   �� 5)      -   5         ��Name$ BdMainWindowSheild��5)   ^   $��0��6   $��0��6)   $��2��66   ���	#70000000$y�	Collapsed=�6z   ._�5*       � -   5        $	 frmPopupWindow��5*      $
 Hidden=�64   $��Center=�6T   $��Center=�6o   5+      5-      5.      5/      50      