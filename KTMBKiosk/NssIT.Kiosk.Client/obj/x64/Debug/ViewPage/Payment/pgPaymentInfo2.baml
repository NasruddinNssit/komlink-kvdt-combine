   M S B A M L   `   `   `  ����M  INssIT.Kiosk.Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullY1clr-namespace:NssIT.Kiosk.Client.ViewPage.Payment#NssIT.Kiosk.Client.ViewPage.Payment  5      8    2NssIT.Kiosk.Client.ViewPage.Payment.pgPaymentInfo2   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Clocal1clr-namespace:NssIT.Kiosk.Client.ViewPage.Payment     5      -   5          �� IsPressAndHoldEnabled  .   N� 
FontFamily$ Roboto)�5	       N� Title$ Payment Summary��5
       � 	Resources 5       �� MergedDictionaries �� Source�� 5   
    5      �� 5      $ rosPaymentEnglish.xaml?�6%   %    5      .��5      �  � RowDefinitions 5   
   �� 5      $��70 �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5       $
��Auto �6   �� 5"      $��* �6   �� 5$      $	��160 �6   �� 5&      $
��Auto �6   .+�5*   
   �� $��0��6   $��36(�6!   ���	#FF777777$��Center=�6F   $��20,0,0,0q�6a      PAYMENT_DETAILS_Label#���   6s   �� 5,   
   $��1��6   ���	#FFEEEEEE$��	20,3,20,3q�65   ._�5-      �� -   5         ��Name$ StkSummaryItem��5-   ,   $��	10,0,10,0q�6   .+�5.      �� $��0��6   $��20(�6)   ���	#FF444444$
��Bold"�6N   $��Center=�6`   $��0,10,0,5q�6{     ORDER_SUMMARY_Label#���  6�   8   2NssIT.Kiosk.Client.ViewPage.Payment.uscSummaryItem  5/      50      51      �� 54   
   $��3��6   ���	#FFE5E5E5$��	20,3,20,3q�65   ._�55        -   5          Name$ 
SitPaySumm��55   6   $��	10,0,10,0q�6#   66   56      �� 59   
   $��4��6   ���	#FFE5E5E5$��	20,3,20,3q�65   ._�5:      � 	 � ColumnDefinitions	 5;      �� 5<      $��* �6'    5>      �� 5?      $��28 �6$   �� 5@      $
��Auto �6$   .+�5B      ��   PAYMENT_OPTIONS_Label#���  6   $��Center=�6K   $��20(�6f   $
��Bold"�6t   ���	#FF444444$��10,0,0,0q�6�   � 5D      -   5        
 �Name$ 
 WrpPaymentGatewayContainer��5D   o    � Orientation$ 
Horizontal=�6   $��Center=�65   $��0��6R   $��1��6b   .+�5E      �� -   5        ���	#FFF3F0F0$��84��5E   5   $	��250��6A    �� CornerRadius$ 10��6M   $��5q�6_   ._�5F      �  5G      �� 5H   "   $��20 �60   �� 5I   "   $��* �60   �� 5J   "   $��10 �60   .+�5L      �� $��Credit / Debit Card��6(   $��12(�6C   $��Bottom=�6Q   $��Center=�6l   �� 5M      $��1��6$   $-��'/Resources/Master_Visa_Debit_Card01.png��61   $��Center=�6b   5N      5O      �� 5P      -   5         ��Name$ BdBoost��5P   �   ���	#FFF3F0F0$��84��64   $	��250��6@   $ 10��6L   $��5q�6^   $y�Visible=�6i   
 � Tag$  ��6~   ._�5Q      �  5R      �� 5S   "   $��20 �60   �� 5T   "   $��* �60   �� 5U   "   $��10 �60   .+�5W      �� -   5         ��Name$ TxtBoostDesc��5W   �   $��Boost (eWallet)��6(   $��12(�6?   $��Bottom=�6M   $��Center=�6h   6�   �� 5X      -   5         ��Name$ imgBoost��5X   s   $��1��6$   $��/Resources/boost.png��61   $��Center=�6O   $  ��6l   6s   5Y      5Z      �� 5[      -	   5        $ BdTng��5[   �   ���	#FFF3F0F0$��84��64   $	��250��6@   $ 10��6L   $��5q�6^   $y�Visible=�6i   $  ��6~   ._�5\      �  5]      �� 5^   "   $��20 �60   �� 5_   "   $��* �60   �� 5`   "   $��10 �60   .+�5b      �� -
   5        $ 
TxtTnGDesc��5b   �   $��Touch n Go (eWallet)��6(   $��12(�6D   $��Bottom=�6R   $��Center=�6m   6�   �� 5c      -   5        $ imgTnG��5c   s   $��1��6$   $��/Resources/TnG00.png��61   $��Center=�6O   $  ��6l   6s   5d      5e      �� 5f      -   5        $ BdLoadingeWallet��5f   i   ���	#FFF3F0F0$��84��64   $	��250��6@   $ 10��6L   $��5q�6^   ._�5g      �  5h      �� 5i   "   $��* �60   .+�5k      �� $ ��Loading eWallet payment ..��6(   $��16(�6J   $��WrapWithOverflow=�6X   $��Center=�6x   $��Center=�6�   5l      5m      5n      5o      5p      � 5s   
   $��6��6   $��	20,3,20,3q�6    5t      �� 5u      $
��Auto �6    �� 5v      $��* �6    �� 5w      $
��Auto �6    .+�5z      �� $��0��6   $P��J*by clicking pay you agree on term and condition of use and privacy policy��6%   $��11(�6w   ���	#FF7D7D7D$��20,0,0,0q�6�    � 5{      -   5          �Name$ frmNav��5{      $��2��6$   $��0,0,0,30q�61   $
��Left=�6C     � NavigationUIVisibility$ Hidden=�6^   5|      5~      5      