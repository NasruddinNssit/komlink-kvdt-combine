   M S B A M L   `   `   `  ����M  INssIT.Kiosk.Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullS.clr-namespace:NssIT.Kiosk.Client.ViewPage.Trip NssIT.Kiosk.Client.ViewPage.Trip  5      O Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem 5      3    -NssIT.Kiosk.Client.ViewPage.Trip.pgTimeFilter   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      @local.clr-namespace:NssIT.Kiosk.Client.ViewPage.Trip     5      6sys&clr-namespace:System;assembly=mscorlib     5      -   5          �� IsPressAndHoldEnabled  .   N� 
FontFamily$ Segoe UI)�5
       N� Title$ pgTimeFilter��5       � 	Resources 5       �� MergedDictionaries �� Source�� 5   
    5      �� 5      $ rosTripEnglish.xaml?�6%   %    5      .��5      � -   5         �Name$ GrdMain��5       5   
      
ThumbWidth  ThumbHeight  TimeFilterHeight  TimeLineBableWidth  TimeLineBableWidth2��%�   &	        &	      &	 4     &	 O     &	 h     X�5      60Q�60   X�5      60Q�61   X�5      360Q�67   X�5      7Q�67   X�5      25Q�69   5       � ColumnDefinitions 5    
   �� 5!      $
��Auto �6   �� 5"      $
��Auto �6   �� 5#      $��* �6   �� 5$      $��* �6    � RowDefinitions 5&   
   �� 5'      $��25 �6   �� 5(      $
��Auto �6   �� 5)      $��60 �6   .+�5,   
   �� -   5        	 ��Name$	 CvSliderContainer��5,      $��0��6,   $��1��6<   #��[  6I   #��[ 6m   $
��Left=�6�   $	��Top=�6�   ���	#01FFFFFF
 K� ZIndex$
 1��6�   .+�5-      ��  �� Left$
 22.5��6   
 �� Top$ 20��6(   $��15��68   $	��320��6C   ���	#FFFFFFFF$��2q�6g   ���	#FF777777 �� CornerRadius$ 5��6�   $
 1��6�   �� 5.      -   5         ��Name$ 	ImgThumb1��5.      $
��Left=�6'   #��[  6B   #��[ 6f   $	��Top=�6�   $ 0��6�   $ 0��6�   $%��/Resources/ThumbFilterFrom4.png��6�   $
��Fill=�6�   $
 2��6�   �� 5/      -   5        $ 	ImgThumb2��5/      $
��Left=�6'   #��[  6B   #��[ 6f   $	��Top=�6�   $ 0��6�   $	 300��6�   $#��/Resources/ThumbFilterTo4.png��6�   $
��Fill=�6�   $
 3��6�   50      �� 52   
   $��0��6   $��0��6$   $��4��61   $��Center=�6E   ���	#FFFFFFFF$��15(�6y   $
��Bold"�6�     TIME_FILTER_Label#���  6�   $��5,0,3,0q�6�   $	��Top=�6�   $��	Underline{�6�   � 54   
   $��1��6   $��1��6   $��	0,23,0,23q�6,   $��80��6?    55      �� 56      $
��Auto �6#    58      �� 59      $��* �6    �� 5:      $��36 �6    �� 5;      $��10 �6    �� 5<      $��36 �6    �� 5=      $��* �6    .+�5@      �� $��0��6   $��1��6)    �� Orientation$ Vertical=�66   $��Bottom=�6M   $��5,0,0,0q�6h   .+�5A      �� $��Bottom=�6   ���	#FFA8A8A8$��13(�6N   $
��Bold"�6\     TIME_FILTER_FROM_Label#���  6n   �� 5B      -   5         ��Name$ TxtFromTime��5B   ~   $��Bottom=�6   ���	#FFF3C100$��15(�6N   $
��Bold"�6\   $��12:00 AM��6n   6~   5C      �� 5E      $��0��6   $��3��6)   $ Vertical=�66   $	��Top=�6M   $��5,0,0,0q�6e   .+�5F      �� $��Bottom=�6   ���	#FFA8A8A8$��13(�6N   $
��Bold"�6\     TIME_FILTER_TO_Label#���  6n   �� 5G      -   5        $ 	TxtToTime��5G   ~   $��Bottom=�6   ���	#FFF3C100$��15(�6N   $
��Bold"�6\   $��11:59 PM��6n   6~   5H      5I      �� 5K   
   -   5        $��0��5K      $��2��6!   $��2��6.   $��Reset��6V   $y�	Collapsed=�6f   �� 5M   
   -	   5        $��0��5M      $��2��6!   $��2��6.   $ 5��6B   ���	#FF919191$��45��6j   $��Bottom=�6w   $��0,0,20,0q�6�   $��Right=�6�   $	��100��6�   ._�5N      ��   TIME_FILTER_APPLY_Label#���  6   
�������$��Center=�6\   $��Center=�6w   $��16(�6�   5O      5R      5S      