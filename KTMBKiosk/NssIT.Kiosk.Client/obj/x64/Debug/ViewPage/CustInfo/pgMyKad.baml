   M S B A M L   `   `   `  ����M  INssIT.Kiosk.Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null[2clr-namespace:NssIT.Kiosk.Client.ViewPage.CustInfo$NssIT.Kiosk.Client.ViewPage.CustInfo  5      2    ,NssIT.Kiosk.Client.ViewPage.CustInfo.pgMyKad   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Dlocal2clr-namespace:NssIT.Kiosk.Client.ViewPage.CustInfo     5      -   5          �� IsPressAndHoldEnabled  .  $	��300��5      $	��600��6    N� 
FontFamily$ Segoe UI)�5	       N� 
Background
 ����� N� Title$ MyKad IC Entry��5
       � 	Resources 5       �� MergedDictionaries �� Source�� 5   
    5      �� 5      $ rosCustInfoEnglish.xaml?�6%   %    5      .��5      �� $��1,1,1,1q�6   ���	#FF0000FF._�5   
   �  � ColumnDefinitions 5      �� 5      $��5 �6#   �� 5      $��* �6#   �� 5      $��* �6#   �� 5      $��* �6#   �� 5      $��* �6#   �� 5      $��5 �6#    � RowDefinitions 5      �� 5      $��5 �6    �� 5       $��* �6    �� 5!      $��* �6    �� 5"      $��* �6    �� 5#      $��5 �6    .+�5&      �� $��1��6   $��1��6)   $��3��66   	 �� Orientation$	 
Horizontal=�6K   $
��Left=�6d   $	��Top=�6   $��40��6�   $	��450��6�   .+�5'      ��    SCAN_PASSENGER_MYKAD_Label#���   6   $��26(�6P   $
��Bold"�6^   �� 5(      -   5        
 ��Name$
 TxtPassengerNo��5(   4   $��20��6   $��26(�6&   $��3,0,0,0q�6L   $
��Bold"�6]   5)      �� 5+      -   5        $
 TxtCountDown��5+   �   $��4��6   $��1��6(   $	��CTD��65   $��48(�6@   $
��Bold"�6N   $��Right=�6`   $	��Top=�6|   $��60��6�   $	��100��6�   �� 5-      $��1��6   $��2��6)   $��4��66   $	 
Horizontal=�6K   $
��Left=�6e   $	��Top=�6�   $��41��6�   .+�5.      �� -   5        $
 TxtInsertMyKad��5.      $��24(�64     INSERT_MYKAD_FOR_SCAN_Label#���  6B   $y�	Collapsed=�6w   �� 5/      -   5        $
 TxtRemoveMyKad��5/      $��24(�64   $��	0,0,0,-60q�6B   $	��100��6U   $	��Top=�6b   �� 50        REMOVE_MYKAD_Label#��  6   �� 51      �� 52        READYFORMOBILE_Label#��  6   53      54      �� 57      $��1��6   $��3��6$   $��2��61   $
��Left=�6E   $"��/Resources/ID-scan-green.png��58      $	��120��66    K� ZIndex$ 10��6B   �� 5:      -   5         ��Name$ 	BdReadLed��5?   '   $��1��5:      $��3��6%   $��2��62   $��5q�5;      ���	#FF00DF00 �� CornerRadius$ 20��5<      $��10��6'   $��10��62   $
��Left=�5=      $	��Top=�60   $��50, 23, 0, 0q�5>      $ 13��5?      6'   �� 5A      $��3��6   $��3��6)   $��2��66   $	 
Horizontal=�6K   $��Right=�6d   $��RightToLeft=�6�   .+�5C      �� -   5        $ 5��5C      ���	#FFFE4C70$��45��6A   $��Bottom=�6N   $��0,0,10,0q�6i   $��Right=�6{    J� RenderTransformOrigin$ 0.5,0.5&�6�   $	��100��6�   ._�5D      �� $��X��6    
�������$��16(�6<   $��Center=�6J   $��Center=�6e   $��50��6�    �� TextAlignment$ Center=�6�   $��0,0,0,1q�6�   5E      �� 5G      -   5        $
 BdOK��5G      $ 5��6'   ���	#FFFBD012$��45��6O   $��Bottom=�6\   $��Right=�6w   $ 0.5,0.5&�6�   $	��100��6�   ._�5H      �� $��OK��6    ���	#FF444444$��16(�6A   $��Center=�6O   $��Center=�6j   $��50��6�   $ Center=�6�   $��0,0,0,1q�6�   5I      5K      5M      5N      5P      