   M S B A M L   `   `   `  ����M  INssIT.Kiosk.Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nulla5clr-namespace:NssIT.Kiosk.Client.ViewPage.PopupWindow'NssIT.Kiosk.Client.ViewPage.PopupWindow  5      7    1NssIT.Kiosk.Client.ViewPage.PopupWindow.pgTimeout   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Glocal5clr-namespace:NssIT.Kiosk.Client.ViewPage.PopupWindow     5      -   5          N� 
FontFamily$  Roboto)�5      $	��450��5      $	��800��6    N� Title$ Timeout Notification��5	       � 	Resources 5       �� MergedDictionaries �� Source�� 5   
    5      �� 5      $ rosPopupWindowEng.xaml?�6%   %    5      .��5      �� ���	#C00000FF �� CornerRadius$ 50��6$   ._�5   
   �  � ColumnDefinitions 5      �� 5      $��20 �6#   �� 5      $��* �6#   �� 5      $��* �6#   �� 5      $��* �6#   �� 5      $��* �6#   �� 5      $��* �6#   �� 5      $��20 �6#    � RowDefinitions 5      �� 5       $��30 �6    �� 5#      $��* �6    �� 5&      $
��0.8* �6    �� 5)      $��2* �6    �� 5,      $
��0.1* �6    �� 5/      $��2* �6    �� 51      $��30 �6    .+�54      �� -   5         ��Name$ TxtTimeoutMessage��54   �   $��1��6   $��1��6(   $��5��65   $��Center=�6I   $��Center=�6d   $��60(�6�   $
��Bold"�6�   ���	#FFDDDDDD$��TIMEOUT_IN_Label��6�   6�   �� 56      -   5        $ TxtCountDown��56   �   $��2��6   $��3��6(   $��3��65   $��Center=�6I   $��Center=�6d   $	��156(�6�   $
��Bold"�6�   ���	#FFFBD012$��30��6�   6�   �� 58      $��1��6   $��5��6)   $��5��66   	 �� Orientation$	 
Horizontal=�6J   $��Bottom=�6c   $��Center=�6~   .+�5:      �� -   5        
 ��Name$
 BdExit��5:      $ 10��6)   ���	#FFFBD012$��90��6R   $��Bottom=�6_   $��Right=�6z   $	��250��6�   $��0,0,10,0q�6�   ._�5<      ��    
EXIT_Label#���   6    ���	#FF272727$��Center=�6[   $��Center=�6v   $��36(�6�   $
��Bold"�6�   5=      �� 5?      -   5        $
 
BdContinue��5?      $ 10��6-   ���	#FFFBD012$��90��6V   $��Bottom=�6c   $��Right=�6~   $	��250��6�   $��10,0,0,0q�6�   ._�5A      �� ���	#FF272727$��Center=�67   $
��Left=�6R   $��24(�6m   $��WrapWithOverflow=�6{   $
��Bold"�6�    �� TextAlignment$ Center=�6�     CONTINUE_Label#���  6�   $��
26,16,0,16q�6�   5B      5C      5D      5E      5F      