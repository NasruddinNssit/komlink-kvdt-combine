   M S B A M L   `   `   `  ����M  INssIT.Kiosk.Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullU/clr-namespace:NssIT.Kiosk.Client.ViewPage.Alert!NssIT.Kiosk.Client.ViewPage.Alert  5      :    4NssIT.Kiosk.Client.ViewPage.Alert.pgUnderMaintenance   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Alocal/clr-namespace:NssIT.Kiosk.Client.ViewPage.Alert     5      -   5          �� IsPressAndHoldEnabled  .   N� 
FontFamily$ Segoe UI)�5
       N� 
Background
 ����� N� Title$ System Under Maintenance��5       � 	Resources 5         btnAsistance �� 
TargetType �� Property �� Value �� 
TargetType	 �� CornerRadius
 ��Name�� 5   
   %}  &	        �� 5      " ��6)   .�5      ��  � ��$ 15(�6-   �� 5       � �� �#7777776/   �� 5       � �� 5      �� 5      " ��6*   .R�5      �� $	 23��6%   ���#CDF7FD._�5   "   �� $
 contentPresenter��63   #��z��5   3   #��z��5   3   #��z��5   3   #��z��5   3   #��z��5   3   5      5      5      5       5!      .��5$      �  � ColumnDefinitions 5%   
   �� 5&      $��40 �6   �� 5'      $��* �6   �� 5(      $
��2.9* �6   �� 5)      $��* �6   �� 5*      $��40 �6    � RowDefinitions 5,   
   �� 5-      $��40 �6   �� 5.      $��70 �6   �� 5/      $��* �6   �� 50      $
��5.1* �6   �� 51      $��* �6   �� 52      $��40 �6   �� 53      $��40 �6   .+�57   
   �� $��0��6   $��5��6    $��0��64   $��7��6A   $��/Resources/TVM-bg.png��58      $��UniformToFill=�60   $��Center=�6H   �� 5=   
   $��1��6   $��5��6    $��	0 0 0 -10q�6-   $
��Left=�6@   $'��!/Resources/NSS-poweredby logo.png��5>      �� 5@   
   -   5         ��Name$ 	TxtSysVer��5B      $��3��5@      $��5��6$   $��xxxxx��61   ���	#FF777777 �� TextAlignment$ Right=�6V   $��Right=�5A      $��Bottom=�68   $��0,0,30,5q�6S   $	��150��6e   $��WrapWithOverflow=�6t   5B      � 5D   
   $��2��6   $��3��6    $��Center=�6-    5E      �� 5F      $��* �6#    5H      �� 5I      $
��Auto �6    �� 5J      $
��Auto �6    .+�5M      (� $��0��6   $��0��6&   .��5N      � .+�5O      �� $ ��/Resources/redwhitebox.png��6   $
��Fill=�6@   $
��1250��6O   $	��650��6\   �� 5Q      $��Center=�6    $ Center=�6=   $��Center=�6T   $��60(�5R      $
��Bold"�6"   �� 5S      -   5         ��Name$ 	TxtMalMsg��5S      $*�$Penyelenggaraan sedang dijalankan ..��6-   ���#0000006Y   �� 5T      �� 5U      -   5        $ 	TxtEngMsg��5U      $ �Maintenance in progress ..��6-   ���#7777776O   5V      �� 5X      -   5        $ TxtProblemMsg��5X       $��Center=�68   $
 Left=�6S   ���#777777$	��300��6}   $	��300��6�   $��80��6�   $��12(�5Y      $
��Bold"�6"   $�� ��64   $��216,377,216,143q�6<   $
��Wrap=�6U   �� 5[      #��[  6   $��Bottom=�6C   $��Right=�6^   $��50��5\      $	��190��6   $��	0,0,54,73q�6)   $y�	Collapsed=�6<   .��5]      �  5^      �� 5_   "   $��2* �63   �� 5`   "   $��* �63   .+�5c      �� $&�� Hello, press here for assistance��6(   $
��Wrap=�6P   $ Center=�6d   $��Center=�6{   �� 5e      $��1��6$   $��/Resources/help-green.png��64   $��0 -2 -22 -9q�6W   5f      5g      5h      5i      �� 5k      $��0��6   $��1��6)    �� Orientation$ Vertical=�66   $	��Top=�6M   .+�5l      �� -   5        $ TxtStartTimeStr��5l      $	��Top=�66   $ Center=�6N   ���	#FF888888$	��300��6|   $	��300��6�   $��80��6�   $��12(�5m      $
��Bold"�6"   $��20201101-133057��64   $
��Wrap=�6K   �� 5o      -   5        $ 
TxtTimeStr��5o      $	��Top=�61   $ Center=�6I   ���	#FF888888$	��300��6w   $	��300��6�   $��80��6�   $��12(�5p      $
��Bold"�6"   $��20201101-133057-64735��64   $
��Wrap=�6Q   5q      5r      �� 5t   
   -   5         ��Name$ BtnFinishMaintenance��5t      $��Finished Maintenance��6/   $��2��6N   $��41,0,0,20.662q�6^   $��4��6u   $
��Left=�6�   $	��275��6�   $��40��6�   $��Bottom=�6�   $y�	Collapsed=�6�   5x      5y      