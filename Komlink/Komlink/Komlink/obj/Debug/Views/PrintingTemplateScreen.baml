   M S B A M L   `   `   `  ����B  >Komlink, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:komlink.Viewskomlink.Views  5      *    $komlink.Views.PrintingTemplateScreen   5      ^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      -localclr-namespace:komlink.Views     5      $��Roboto)�5        � 	Resources  5
         BtnPrint �� 
TargetType �� Property �� Value �� 
TargetType �� CornerRadius  
BtnContact��%�  &	        &	 �    �� 5   
   " ��6!   .�5      ��  � �� �#0000006=   �� 5       � ��$
 Bold"�6+   6:   �� 5       � ��$ 30(�6)   66   �� 5       �@�Cursor$
 Hand��6'   66   �� 5       � �� 5      �� 5      " ��6&   .R�5      �� ���#43D82C$ 11��66   $��80��6H   $	��270��6T   $��Stretch=�6a   $��Center=�6   ._�5      �� #��z��6/   $��Center=�6S   $��Center=�6n   6�   5      5      5      5      �� 5   
   " ��6#   .�5      ��  � �� �#0000006=   �� 5       � ��$
 Bold"�6+   6:   �� 5       � ��$ 25(�6)   66   �� 5       �@�Cursor$
 Hand��6'   66   �� 5       � �� 5       �� 5!      " ��6&   .R�5"      �� ���#f22cae$ 11��66   $��80��6H   $	��270��6T   $��Stretch=�6a   $��Center=�6   ._�5#      �� #��z��6/   $��Center=�6S   $��Center=�6n   6�   5$      5%      5'      5(      5)      .��5*      �� .��5+   
   �  � RowDefinitions 5,      �� 5-      $
��auto �6    60   �� 5.      6"    � ColumnDefinitions 50      �� 51      6%   �� 52      6%   .+�54      �� $��2��6   $��/Assets/ktmheader.png��6(   $��UniformToFill=�6G   6a   � 55      $��1��6   $��Center=�6    $��Center=�6=    56      �� 57      $
��auto �6$   64   �� 58      $
��auto �6$   64   �� 59      $
��auto �6$   64   �� 5:      $
��auto �6$   64   .+�5<      �� -   5         ��Name$ lblPrinting��5<      $��Printing Your Ticket��61   	 �� TextAlignment$	 Center=�6M   $
��Bold"�6d   $��45(�6v   $��	0,10,0,10q�6�   6�   �� 5=      -   5        $ 
lblTransNo��5=      $��1��60   $��Trans No��6=   $	 Center=�6M   $
��Bold"�6d   $��36(�6v   $��0,7,0,7q�6�   6�   �� 5>      -   5        $ lblThankYou��5>      $��2��61   $!��Thank You, Have A Nice Trip��6>   $	 Center=�6a   $
��Bold"�6x   $��25(�6�   $��0,5,0,5q�6�   6�   �� 5?      -   5        $ 
lblNiceDay��5?      $��3��60   $��Have A Nice Day !��6=   $	 Center=�6V   $
��Bold"�6m   $��25(�6   $��0,5,0,5q�6�   6�   5@      � 5A      $��1��6   $��1��6     5B      �� 5C      6&   �� 5D      6&   .+�5F      �� -   5        
 ��Name$
 BtnPrint��5F      #��[  6+   $��PRINT RECEIPT��6M    � Cursor$
 Hand��6f   $��Center=�6t   $��Bottom=�6�   $��0,0,0,20q�6�   �� 5G      -   5        $
 
BtnContact��5G      $��1��6-   #��[ 6:   
�������$��CONTACT SUPPORT��6q   $
 Hand��6�   $��Center=�6�   $	��Top=�6�   $��0,0,0,20q�6�   5H      5I      5J      5K      