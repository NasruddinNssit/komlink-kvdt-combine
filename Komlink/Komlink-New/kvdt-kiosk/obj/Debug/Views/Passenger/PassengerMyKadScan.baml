   M S B A M L   `   `   `  ����E  Akvdt-kiosk, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullG(clr-namespace:kvdt_kiosk.Views.Passengerkvdt_kiosk.Views.Passenger  5      3    -kvdt_kiosk.Views.Passenger.PassengerMyKadScan   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      :local(clr-namespace:kvdt_kiosk.Views.Passenger     5      -   5        $��Inter)�5        � 	Resources  5	         BtnReset �� 
TargetType �� Property �� Value �� 
TargetType �� CornerRadius  BtnSelected��%�  &	        &	 �    �� 5
   
   " ��6!   .�5      ��  � �� �#0000006=   �� 5       � ��$
 Bold"�6+   6:   �� 5       � ��$ 15(�6)   66   �� 5       �@�Cursor$
 Hand��6'   66   �� 5       � �� 5      �� 5      " ��6&   .R�5      �� ���#CCCCCC$ 11��66   $��50��6H   $	��150��6T   $��Stretch=�6a   $��Center=�6   ._�5      �� #��z��6/   $��Center=�6S   $��Center=�6n   6�   5      5      5      5      �� 5   
   " ��6$   .�5      ��  � �� �#0000006=   �� 5       � ��$
 Bold"�6+   6:   �� 5       � ��$ 15(�6)   66   �� 5       �@�Cursor$
 Hand��6'   66   �� 5       � �� 5      �� 5       " ��6&   .R�5!      �� ���#FBD012$ 15��66   ._�5"      �� #��z��6/   $��Center=�6S   $��Center=�6n   6�   5#      5$      5&      5'      5(      .��5*      �� .��5+   
   � ���#074481$��0,0,0,0q�6$    � RowDefinitions 5,      �� 5-      $
��auto �6    60   �� 5.      $
��auto �6    60   �� 5/      $
��auto �6    60   �� 50      6"    � ColumnDefinitions 52      �� 53      6%   �� 54      $
��auto �6#   62   �� 55      $
��auto �6#   62   .+�57      �� -   5         ��Name$ lblPassenger��57      $��
PASSENGERS��6.   ���#FFFFFF$��42(�6U   $
��Left=�6c   $��Center=�6~   $
��Bold"�6�   $��
20,10,0,10q�6�   6�   �� 58      -   5        	 ��Name$	 BtnScan��58      $��0��6&   $��1��63   #��[  6C   $��
SCAN MYKAD��6e   $��5,15,5,0q�6z   $	��160��6�   $��50��6�   $	��Top=�6�   �� 59      -   5        $	 BtnReset��59      $��0��6'   $��2��64   #��[  6D   $��RESET��6f   $��35,15,35,10q�6v   6�   � 5;      $��1��6   $��3��6    $��0��64   $��0,0,0,0q�6D    5<      �� 5=      6&   �� 5>      6&   �� 5?      6&   �� 5@      6&   �� 5A      6&   �� 5B      6&   .+�5D      �� -   5        $ lblChild��5D      $��KANAK-KANAK / CHILD��6.   $��25(�6I   ���#FFFFFF$
��Bold"�6l   $��	25,18,2,2q�6~   6�   A� 5E      -   5        
 A�Name$
 GridChildren��5E   +   $��1��6   5G      �� 5I      -   5        $ 	lblSenior��5I      $��2��6/   $!��WARGA EMAS / SENIOR CITIZEN��6<   $��25(�6_   ���#FFFFFF$
��Bold"�6�   $��	25,18,2,2q�6�   6�   A� 5J      -   5        $
 
GridSenior��5J   +   $��3��6   5L      �� 5N      -	   5        $ lblOKU��5N      $��4��6,   $,��&Orang Kurang Upaya (OKU) / OKU CITIZEN��69   $��25(�6g   ���#FFFFFF$
��Bold"�6�   $��	25,18,2,2q�6�   6�   A� 5O      -
   5        $
 GridOKU��5O   +   $��5��6   5Q      5R      5S      5T      5U      