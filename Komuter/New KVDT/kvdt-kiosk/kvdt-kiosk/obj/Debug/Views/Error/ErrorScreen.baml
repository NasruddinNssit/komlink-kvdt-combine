   M S B A M L   `   `   `  ����E  Akvdt-kiosk, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null?$clr-namespace:kvdt_kiosk.Views.Errorkvdt_kiosk.Views.Error  5      (    "kvdt_kiosk.Views.Error.ErrorScreen   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      6local$clr-namespace:kvdt_kiosk.Views.Error     5      $��Inter)�5        � 	Resources  5	         	BtnYellow �� 
TargetType �� Property �� Value �� 
TargetType �� CornerRadius��%�  &	        �� 5
   
   " ��6"   .�5      ��  � �� �#0000006=   �� 5       � ��$
 Bold"�6+   6:   �� 5       � ��$ 20(�6)   66   �� 5       �@�Cursor$
 Hand��6'   66   �� 5       � �� 5      �� 5      " ��6&   .R�5      �� ���#FBD012$ 7��66   ._�5      �� #��z��6/   $��Center=�6S   $��Center=�6n   6�   5      5      5      5      5      .��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6    � ColumnDefinitions 5    
   �� 5!      $
��Auto �6   �� 5"      $��* �6   .+�5%   
   ��  �� Orientation$ 
Horizontal=�6   $��0��6.   .+�5&      �� $	��150��6   $	��150��6    {�.  $��/Assets/close.png��6?   $��UniformToFill=�6Z   �� 5'      -   5        	 ��Name$	 lblTitle��5'      $��Title��6*   $��Center=�67   $��Center=�6R   $��72(�6o   $
��Bold"�6}   $��15,0,0,0q�6�   5(      �� 5*   
   -   5        $	 lblErrorMessage��5*      $��Error Message��6-   $��WrapWithOverflow=�6B   $��Center=�6b   $��Center=�6   $��1��6�   $��2��6�   $��50(�6�   �� 5,   
   -   5        
 ��Name$
 BtnClose��5,      #��[  6#   $��Close��6F   $��80��6V   $	��180��6b   $��2��6n   $��Center=�6{   $��10,40,10,40q�6�   6�   5-      5.      