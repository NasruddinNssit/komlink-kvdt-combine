   M S B A M L   `   `   `  ����M  INssIT.Kiosk.Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullY1clr-namespace:NssIT.Kiosk.Client.ViewPage.Komuter#NssIT.Kiosk.Client.ViewPage.Komuter  5      ?    9NssIT.Kiosk.Client.ViewPage.Komuter.pgKomuterMyKadScanner   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Clocal1clr-namespace:NssIT.Kiosk.Client.ViewPage.Komuter     5      -   5          �� IsPressAndHoldEnabled  .  $	��300��5      $	��600��6    N� 
FontFamily$ Segoe UI)�5	       N� 
Background
 ����� N� Title$ Komuter MyKad IC Scanning��5
       � 	Resources 5       �� MergedDictionaries �� Source�� 5   
    5      �� 5      $ rosKomuterEng.xaml?�6%   %    5      .��5      �� $��1,1,1,1q�6   ���	#FF0000FF._�5   
   �  � ColumnDefinitions 5      �� 5      $��5 �6#   �� 5      $��* �6#   �� 5      $��* �6#   �� 5      $��* �6#   �� 5      $��* �6#   �� 5      $��5 �6#    � RowDefinitions 5      �� 5      $��5 �6    �� 5       $��* �6    �� 5!      $��* �6    �� 5"      $��* �6    �� 5#      $��5 �6    .+�5&      �� $��1��6   $��1��6)   $��3��66   	 �� Orientation$	 Vertical=�6K   $
��Left=�6b   $	��Top=�6}   $	��450��6�   .+�5'      �� -   5        
 ��Name$
 TxtTitle��5'   =   $��Ticket Type��6   $��26(�6/   $��3,0,0,0q�6O   $
��Bold"�6`   $��CharacterEllipsis=�6r   �� 5(      $	 
Horizontal=�6   $
��Left=�66   $	��Top=�6Q   $��40��6i   $	��450��6u   .+�5)      �� -   5        $
 
TxtScanTag��5)   t      SCAN_PASSENGER_MYKAD_Label#���   6    $��26(�6T   $
��Bold"�6b   6t   �� 5*      -   5        $
 TxtPassengerNo��5*   8   $��20��6    $��26(�6*   $��3,0,0,0q�6P   $
��Bold"�6a   5+      5,      �� 5.      -   5        $
 TxtCountDown��5.   �   $��4��6   $��1��6(   $	��CTD��65   $��48(�6@   $
��Bold"�6N   $��Right=�6`   $	��Top=�6|   $��60��6�   $	��100��6�   �� 50      $��1��6   $��2��6)   $��4��66   $	 
Horizontal=�6K   $
��Left=�6d   $	��Top=�6   .+�51      �� -   5        $
 TxtErrorMsg��51      
���  ��$��24(�6B   $�� ��6P   $	��500��6X   $��WrapWithOverflow=�6g   $y�	Collapsed=�6�   �� 52      -   5        $
 TxtInsertMyKad��52      $��24(�64     INSERT_MYKAD_FOR_SCAN_Label#���  6B   $y�Visible=�6w   �� 53      -   5        $
 TxtRemoveMyKad��53      $��24(�64   $��	0,0,0,-60q�6B   $	��100��6U   $	��Top=�6b   $y�	Collapsed=�6z   �� 54      -	   5         ��Name$ TxtRemoveMyKad1��54   F     REMOVE_MYKAD_Label#��  6   6F   �� 55      �� 56      -
   5        $ TxtRemoveMyKad2��56   G     REMOVE_MYKAD2_Label#��  6   6G   57      58      �� 5:      $��1��6   $��3��6$   $��2��61   $
��Left=�6E   $"��/Resources/ID-scan-green.png��5;      $	��120��66   �� 5=      $��3��6   $��3��6)   $��2��66   $	 
Horizontal=�6K   $��Right=�6d   $��RightToLeft=�6�   .+�5?      �� -   5         �� CornerRadius$ 5��5?      ���	#FFFE4C70$��45��6A   $��Bottom=�6N   $��0,0,10,0q�6i   $��Right=�6{    J� RenderTransformOrigin$ 0.5,0.5&�6�   $	��100��6�   ._�5@      �� $��X��6    
�������$��16(�6<   $��Center=�6J   $��Center=�6e   $��50��6�    �� TextAlignment$ Center=�6�   $��0,0,0,1q�6�   5A      �� 5C      -   5         ��Name$
 BdOK��5C      $ 5��6'   ���	#FFFBD012$��45��6O   $��Bottom=�6\   $��Right=�6w   $ 0.5,0.5&�6�   $	��100��6�   ._�5D      �� $��OK��6    ���	#FF444444$��16(�6A   $��Center=�6O   $��Center=�6j   $��50��6�   $ Center=�6�   $��0,0,0,1q�6�   5E      5G      5I      5J      5K      