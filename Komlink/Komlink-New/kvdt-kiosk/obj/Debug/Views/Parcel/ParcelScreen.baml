   M S B A M L   `   `   `  ����E  Akvdt-kiosk, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullA%clr-namespace:kvdt_kiosk.Views.Parcelkvdt_kiosk.Views.Parcel  5      *    $kvdt_kiosk.Views.Parcel.ParcelScreen   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      7local%clr-namespace:kvdt_kiosk.Views.Parcel     5      $��Intel)�5        � 	Resources  5	         SeatingTitleStyle �� 
TargetType �� Property �� Value  SeatingDateStyle��%�  &	        &	 �     �� 5
   
   " ��6*   .�5      ��  � �� �#fff6+   �� 5       � ��$
 Bold"�6+   �� 5       � ��$ 30(�6*   �� 5       � ��$ Bottom=�63   �� 5       � ��$ Center=�64   5      �� 5   
   " ��6)   .�5      ��  � �� �#fff6+   �� 5       � ��$ Center=�65   �� 5       � ��$	 Top=�62   �� 5       � ��$ 22(�6)   5      5      .��5      � -   5         �Name$
 Main��5       ���#074481 � RowDefinitions 5   
   �� 5      $
��auto �6   �� 5      $
��auto �6   �� 5      $
��auto �6   �� 5      $
��auto �6   .+�5    
   �� -   5         ��Name$ SeatingTitleText��5       $��Parcel��6.   #��[  6<   $��	0,10,0,10q�6g   $��36(�6z   �� 5!   
   -   5        $��DateNow��5!   !   $��1��6   #��[ 60   $��Initial Text��6Z   $��	0,10,0,10q�6n   A� 5"   
   -   5        $��ParcelContainer��5"   #   $��2��6    A� Rows$ 2��6:   5$      � 5%   
   $��3��6   $��
0,25,15,25q�6   .+�5&      0   *kvdt_kiosk.Views.Parcel.ParcelActionButton  6)   5'      5(      5)      