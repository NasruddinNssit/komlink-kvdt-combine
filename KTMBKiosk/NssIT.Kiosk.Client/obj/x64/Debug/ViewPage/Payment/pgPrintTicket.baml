   M S B A M L   `   `   `  ����M  INssIT.Kiosk.Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullY1clr-namespace:NssIT.Kiosk.Client.ViewPage.Payment#NssIT.Kiosk.Client.ViewPage.Payment  5      7    1NssIT.Kiosk.Client.ViewPage.Payment.pgPrintTicket   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Clocal1clr-namespace:NssIT.Kiosk.Client.ViewPage.Payment     5      -   5          �� IsPressAndHoldEnabled  .   N� 
FontFamily$ Segoe UI)�5	       N� 
Background
 ����� N� Title$ Ticket Printing��5
       � 	Resources 5         btnChangePay �� 
TargetType �� Property �� Value �� 
TargetType	 �� CornerRadius
 ��Name  btnDone  btnDoneDisabled��%�  &	        &	 $    &	 r    �� 5   
   " ��6%   .�5      ��  � �� 5      �� 5      " ��6&   .R�5      �� $	 8��6!   ���#2B9CDB._�5      �� $
 contentPresenter��6/   #��z��5   3   #��z��5   3   #��z��5   3   #��z��5   3   #��z��5   3   5      5      5      5      �� 5   
   " ��6    .�5      ��  � �� �#FFFFFF6+   �� 5        � �� 5!      �� 5"      " ��6&   .R�5#      �� $	 8��6!   ���#F48220._�5$      �� $
 contentPresenter��6/   #��z��5%   3   #��z��5&   3   #��z��5'   3   #��z��5(   3   #��z��5)   3   5*      5+      5-      5.      �� 50   
   " ��6(   .�51      ��  � �� �#FFFFFF6+   �� 52       � �� 53      �� 54      " ��6&   .R�55      �� $	 8��6!   ���#999999._�56      �� $
 contentPresenter��6/   #��z��57   3   #��z��58   3   #��z��59   3   #��z��5:   3   #��z��5;   3   5<      5=      5?      5@      5C      .��5E      �  � ColumnDefinitions 5F   
   �� 5G      $
��1.5* �6   �� 5H      $��* �6    � RowDefinitions 5J   
   �� 5K      $��50 �6   �� 5L      $��* �6   .+�5P   
   �� $��0��6   $��2��6$   $#��Ticket Printing In Progress..��68   ���#777777$��Roboto)�5Q      $��22(�6(   $��Bottom=�66   $��0 0 0 10q�6Q   � 5T   
   $��2��6   $��1��6#   $��2��60   ���#D2E9F5$��0 10 0 0q�6V    5U      �� 5V      $��* �6    �� 5W      $��* �6    �� 5X      $��* �6    �� 5Y      $��* �6     5[      �� 5\      $
��1.5* �6#   �� 5]      $��* �6#   �� 5^      $��* �6#   .+�5b      �� -   5         ��Name$ 
TxtMessage��5e      $��0��5b      $��0��6(   $��3��65   $��2��6I   $��Center=�5c      $��18(�63   ���#777777$��	20 0 20 0q�6V   $��WrapWithOverflow=�6i   $#��Please collect your ticket ..��5d      5e      � 5h      $��3��6    5i      �� 5j      $��2* �6'   �� 5k      $��* �6'   .+�5o      �� -   5         ��Name$ BtnDone��5o   �   
 � Tag$ 0��6   #��[ 6!   $
��DONE��6J   $	��110��6Y   $��30��6e   $
��Left=�6q   $��20,0,0,0q�6�   6�   5p      5r      5s      5t      