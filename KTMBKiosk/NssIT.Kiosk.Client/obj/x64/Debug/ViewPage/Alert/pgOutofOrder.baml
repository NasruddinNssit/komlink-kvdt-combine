   M S B A M L   `   `   `  ����M  INssIT.Kiosk.Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullU/clr-namespace:NssIT.Kiosk.Client.ViewPage.Alert!NssIT.Kiosk.Client.ViewPage.Alert  5      4    .NssIT.Kiosk.Client.ViewPage.Alert.pgOutofOrder   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Alocal/clr-namespace:NssIT.Kiosk.Client.ViewPage.Alert     5      -   5          �� IsPressAndHoldEnabled  .   N� 
FontFamily$ Segoe UI)�5
       N� 
Background
 ����� N� Title$ Out of Order��5       � 	Resources 5         btnAsistance �� 
TargetType �� Property �� Value �� 
TargetType	 �� CornerRadius
 ��Name�� 5   
   %}  &	        �� 5      " ��6)   .�5      ��  � ��$ 15(�6-   �� 5       � �� �#7777776/   �� 5       � �� 5      �� 5      " ��6*   .R�5      �� $	 23��6%   ���#CDF7FD._�5   "   �� $
 contentPresenter��63   #��z��5   3   #��z��5   3   #��z��5   3   #��z��5   3   #��z��5   3   5      5      5      5       5!      .��5$      �  � ColumnDefinitions 5%   
   �� 5&      $��40 �6   �� 5'      $��* �6   �� 5(      $
��3.9* �6   �� 5)      $��* �6   �� 5*      $��40 �6    � RowDefinitions 5,   
   �� 5-      $��40 �6   �� 5.      $��70 �6   �� 5/      $��* �6   �� 50      $
��5.1* �6   �� 51      $��* �6   �� 52      $��40 �6   �� 53      $��40 �6   .+�57   
   �� $��0��6   $��5��6    $��0��64   $��7��6A   $��/Resources/TVM-bg.png��58      $��UniformToFill=�60   $��Center=�6H   � 5:   
   $��2��6   $��3��6    $��Center=�6-    5;      �� 5<      $��* �6#    5>      �� 5?      $
��Auto �6    �� 5@      $
��Auto �6    .+�5C      (� $��0��6   $��0��6&   .��5D      � .+�5E      �� $ ��/Resources/redwhitebox.png��6   �� 5G      $��Center=�6     �� TextAlignment$ Center=�6=   $��Center=�6T   $��60(�5H      $
��Bold"�6"   �� 5I      -   5         ��Name$ 	TxtMalMsg��5I      $�TIDAK BERFUNGSI��6-   ���#0000006D   �� 5J      �� 5K      -   5        $ 	TxtEngMsg��5K      $�OUT OF ORDER��6-   ���#7777776A   5L      �� 5N      -   5         ��Name$ TxtProblemMsg��5N       $	��Top=�68   $ Center=�6P   ���#777777$	��593��6|   $	��160��6�   $��16(�5O      $
��Bold"�6"   $�� ��64   $��	0,340,0,0q�6<   $
��Wrap=�6O   $��CharacterEllipsis=�6c   �� 5Q      #��[  6   $��Bottom=�6C   $��Right=�6^   $��50��5R      $	��190��6   $��	0,0,54,73q�6)   $y�	Collapsed=�6<   .��5S      �  5T      �� 5U   "   $��2* �63   �� 5V   "   $��* �63   .+�5Y      �� $&�� Hello, press here for assistance��6(   $
��Wrap=�6P   $ Center=�6d   $��Center=�6{   �� 5[      $��1��6$   $��/Resources/help-green.png��64   $��0 -2 -22 -9q�6W   5\      5]      5^      5_      �� 5a      $��0��6   $��1��6)    �� Orientation$ Vertical=�66   $��Center=�6M   $	��Top=�6j   .+�5b      �� -   5        $ TxtStartTimeStr��5b      $	��Top=�66   $ Center=�6N   ���	#FF888888$	��300��6|   $��80��6�   $��12(�5c      $
��Bold"�6"   $��20201101-133057��64   $
��Wrap=�6K   �� 5e      -   5        $ 
TxtTimeStr��5e      $	��Top=�61   $ Center=�6I   ���	#FF888888$	��300��6w   $��80��6�   $��12(�5f      $
��Bold"�6"   $��20201101-133057-64735��64   $
��Wrap=�6Q   5g      5h      �� 5j   
   $��1��6   $��5��6    $��	0 0 0 -10q�6-   $
��Left=�6@   $'��!/Resources/NSS-poweredby logo.png��5k      �� 5m   
   -   5        $ 	TxtSysVer��5o      $��3��5m      $��5��6$   $��xxxxx��61   ���	#FF777777$ Right=�6V   $��Right=�5n      $��Bottom=�68   $��0,0,30,5q�6S   $	��150��6e   $��WrapWithOverflow=�6t   5o      5q      5r      