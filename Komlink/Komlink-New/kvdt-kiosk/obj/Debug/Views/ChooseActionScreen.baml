   M S B A M L   `   `   `  ����E  Akvdt-kiosk, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:kvdt_kiosk.Viewskvdt_kiosk.Views  5      )    #kvdt_kiosk.Views.ChooseActionScreen   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      0localclr-namespace:kvdt_kiosk.Views     5      $��Inter)�5        � 	Resources  5	         ButtonBackground �� 
StartPoint �� EndPoint � Color � Offset  GreenButton �� 
TargetType �� Property �� Value �� 
TargetType	 �� CornerRadius
 ��Name�� 5
   
   %]  &	        &	 �     7   �� 5      $ 0.5,0&�6;   $ 0.5,1&�6N   .��5      � $ #FDE371��6   $ 0Q�6/   � 5      $ #FCDA44��6   $ 0.445Q�6/   � 5      $ #FbD012��6   $	 0.8Q�6/   5      �� 5      " ��6(   .�5      ��  � ��$ Center=�6?   �� 5       � ��$ 20(�6-   �� 5       � ��
 �   �6/   �� 5       � �� 5      �� 5      " ��6*   .R�5      �� $	 15��6%   8��  ._�5   "   �� $
 contentPresenter��63   #��z��6M   #��z��6�   #��z��6�   #��z��6�   #��z��6  5      5      5      5      5      .��5       �� .��5!   
   �  � ColumnDefinitions 5"      �� 5#      6%   �� 5$      6%   .+�5&      �� $��UniformToFill=�6   $��/Assets/kvdtmap.png��6,   6L   � 5'      $��1��6   .+�5(      �  � RowDefinitions 5)      �� 5*      $	��588 �6(   67   �� 5+      $	��200 �6(   67   �� 5,      $	��200 �6(   67   .+�5.      �� -   5         ��Name$ 	TxtAction��5.       $��40(�63   $
��Bold"�6A   $��Bottom=�6T   $��Please Choose Your Action��6o   $��0,0,0,47q�6�   $
��Wrap=�6�   $	��336��6�   $	��130��6�    �� TextAlignment$ Center=�6�   6�   �� 5/      -   5         ��Name$ BtnPurchaseTicket��5/      $��1��68   $��	0,0,10,40q�6E   #��[ 6X   $	��320��6�    � Cursor$
 Hand��6�   .��50      �� -   5        $ lblPurchase��50   $   $
��Bold"�69   $��32(�6K   �� 6Z   $�	 Purchase��6^   �� 6q   �� 6}   $�
    Ticket��6�   6�   51      �� 52      -   5        $ 	BtnParcel��52      $��2��60   $��	0,40,10,0q�6=   #��[ 6P   $	��320��6�   $
 Hand��6�   .��53      �� -   5        $ 	lblParcel��53   $   $
��Bold"�67   $��32(�6I   �� 6X   $�
  Purchase��6\   �� 6p   �� 6|   $�Parcel Only��6�   6�   54      55      56      57      58      5:      