   M S B A M L   `   `   `  ����E  Akvdt-kiosk, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullE'clr-namespace:kvdt_kiosk.Views.Printingkvdt_kiosk.Views.Printing  5      6    0kvdt_kiosk.Views.Printing.PrintingTemplateScreen   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      9local'clr-namespace:kvdt_kiosk.Views.Printing     5      -   5          � 	Resources  5	         BtnPrint �� 
TargetType �� Property �� Value �� 
TargetType �� CornerRadius  
BtnContact��%�  &	        &	 �    �� 5
   
   " ��6!   .�5      ��  � �� �#0000006=   �� 5       � ��$
 Bold"�6+   6:   �� 5       � ��$ 30(�6)   66   �� 5       �@�Cursor$
 Hand��6'   66   �� 5       � �� 5      �� 5      " ��6&   .R�5      �� ���#43D82C$ 11��66   $��80��6H   $	��270��6T   $��Stretch=�6a   $��Center=�6   ._�5      �� #��z��6/   $��Center=�6S   $��Center=�6n   6�   5      5      5      5      �� 5   
   " ��6#   .�5      ��  � �� �#0000006=   �� 5       � ��$
 Bold"�6+   6:   �� 5       � ��$ 25(�6)   66   �� 5       �@�Cursor$
 Hand��6'   66   �� 5       � �� 5      �� 5       " ��6&   .R�5!      �� ���#f22cae$ 11��66   $��80��6H   $	��270��6T   $��Stretch=�6a   $��Center=�6   ._�5"      �� #��z��6/   $��Center=�6S   $��Center=�6n   6�   5#      5$      5&      5'      5(      .��5)      �� .��5*   
   �  � RowDefinitions 5+      �� 5,      $
��auto �6    60   �� 5-      6"    � ColumnDefinitions 5/      �� 50      6%   �� 51      6%   .+�53      �� $��2��6   $��/Assets/ktmheader.png��6(   $��UniformToFill=�6G   6a   � 54      $��1��6   $��Center=�6    $��Center=�6=    55      �� 56      $
��auto �6$   64   �� 57      $
��auto �6$   64   �� 58      $
��auto �6$   64   �� 59      $
��auto �6$   64   .+�5;      �� -   5         ��Name$ lblPrinting��5;      $��Printing Your Ticket��61   	 �� TextAlignment$	 Center=�6M   $
��Bold"�6d   $��45(�6v   $��	0,10,0,10q�6�   6�   �� 5<      -   5        $ 
lblTransNo��5<      $��1��60   $��Trans No��6=   $	 Center=�6M   $
��Bold"�6d   $��36(�6v   $��0,7,0,7q�6�   6�   �� 5=      -   5        $ lblThankYou��5=      $��2��61   $!��Thank You, Have A Nice Trip��6>   $	 Center=�6a   $
��Bold"�6x   $��25(�6�   $��0,5,0,5q�6�   6�   �� 5>      -   5        $ 
lblNiceDay��5>      $��3��60   $��Have A Nice Day !��6=   $	 Center=�6V   $
��Bold"�6m   $��25(�6   $��0,5,0,5q�6�   6�   5?      � 5@      $��1��6   $��1��6     5A      �� 5B      6&   �� 5C      6&   .+�5E      �� -   5        
 ��Name$
 BtnPrint��5E      #��[  6+   $��PRINT RECEIPT��6M    � Cursor$
 Hand��6f   $��Center=�6t   $��Bottom=�6�   $��0,0,0,20q�6�   6�   �� 5F      -   5        $
 
BtnContact��5F      $��1��6-   #��[ 6:   
�������$��CONTACT SUPPORT��6q   $
 Hand��6�   $��Center=�6�   $	��Top=�6�   $��0,0,0,20q�6�   5G      5H      5I      5J      