   M S B A M L   `   `   `  ����E  Akvdt-kiosk, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null?$clr-namespace:kvdt_kiosk.Views.MyKadkvdt_kiosk.Views.MyKad  5      2    ,kvdt_kiosk.Views.MyKad.MyKadValidationScreen   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      6local$clr-namespace:kvdt_kiosk.Views.MyKad     5      $��Inter)�5        � 	Resources  5	         BtnClose �� 
TargetType �� Property �� Value �� 
TargetType �� CornerRadius 	 BtnOk��%P  &	        &	 �    �� 5
   
   " ��6!   .�5      ��  � �� �#0000006=   �� 5       � ��$
 Bold"�6+   6:   �� 5       � ��$ 15(�6)   66   �� 5       �@�Cursor$
 Hand��6'   66   �� 5       � �� 5      �� 5      " ��6&   .R�5      �� ���#FF0000$ 15��66   ._�5      �� #��z��6/   $��Center=�6S   $��Center=�6n   6�   5      5      5      5      �� 5   
   " ��6   .�5      ��  � �� �#0000006=   �� 5       � ��$
 Bold"�6+   6:   �� 5       � ��$ 15(�6)   66   �� 5       �@�Cursor$
 Hand��6'   66   �� 5       � �� 5      �� 5       " ��6&   .R�5!      �� ���#546918$ 15��66   ._�5"      �� #��z��6/   $��Center=�6S   $��Center=�6n   6�   5#      5$      5&      5'      5(      .��5)      �  � RowDefinitions 5*   
   �� 5+      6   �� 5,      6   �� 5-      6   �� 5.      6    � ColumnDefinitions 50   
   �� 51      $��* �6   6+   �� 52      $
��auto �6   6.   .+�54   
   �� -   5         ��Name$ lblPassengerInfo��54      $��Passenger Info��6.   	 �� TextAlignment$	 Center=�6D   $
��Left=�6[   $��Center=�6v   $��36(�6�   $
��Bold"�6�   $��10,0,0,0q�6�   6�   �� 55   
   -   5        $ TxtCount��55      $��1��6&   $��30��66   $	 Center=�6@   $��Center=�6W   $��Center=�6t   $��36(�6�   $
��Bold"�6�   6�   �� 57   
   -   5        $ TxtScanText��57      $��1��6)   $��SCAN MYKAD: Passenger��66   $	 Center=�6S   $
��Left=�6j   $��Center=�6�   $��24(�6�   $
��Bold"�6�   $��10,0,0,0q�6�   6�   �� 58   
   -   5        $ TxtInstruction��58      $��2��6,   $)��#Please insert MyKad into the reader��69   $	 Center=�6d   $
��Left=�6{   $��Center=�6�   $��24(�6�   $��10,0,0,0q�6�   $��WrapWithOverflow=�6�   6�   �� 5:   
   $��3��6   $��/Assets/ID-scan-green.png��6   $��50��6@   $
��Left=�6L   $��15,0,0,0q�6g   �� 5;   
   -   5        
 ��Name$
 BtnClose��5;      $��3��6#   $��1��60   ���#FFFFFF$��35(�6U   $��X��6c   #��[  6o   $��Center=�6�   $��50��6�   $	��100��6�   $��Center=�6�   $��7,0,7,0q�6�   6  �� 5<   
   -   5        $
 BtnOK��5<      $y�	Collapsed=�6    $��3��67   $��1��6D   ���#FFFFFF$��35(�6i   $��OK��6w   #��[ 6�   $��Center=�6�   $��50��6�   $	��100��6�   $��Center=�6�   $��7,0,7,0q�6�   6  5=      5>      