   M S B A M L   `   `   `  ����M  INssIT.Kiosk.Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nulle7clr-namespace:NssIT.Kiosk.Client.ViewPage.TicketSummary)NssIT.Kiosk.Client.ViewPage.TicketSummary  5      :    4NssIT.Kiosk.Client.ViewPage.TicketSummary.pgTickSumm   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Ilocal7clr-namespace:NssIT.Kiosk.Client.ViewPage.TicketSummary     5      -   5          N� 
FontFamily$  Roboto)�5       �� IsPressAndHoldEnabled .   N� Title$ Ticket Summary��5       � 	Resources 5       �� MergedDictionaries �� Source�� 5   
    5      �� 5      $ rosTickSummEng.xaml?�6%   %    5      .��5      �  5   
      S1 �� 
TargetType �� Property �� Value	 �� EndPoint
 � MappingMode �� 
StartPoint � Color � Offset  S2  S3�� 5      %�  &	        &	 s    &	 �    �� 5      " �6#   .�5      ��  � �� 5      �� 5      $	 0.5,1&�62   $
 RelativeToBoundingBox=�6C   $ 0.5,0&�6g   .��5   "   � $ 	#FF064C90��6/   $ 0Q�6A   � 5   "   $ 	#FF074F92��6/   $
 0.33Q�6A   � 5   "   $ 	#FF0B599B��6/   $
 0.66Q�6A   � 5    "   $ 	#FF0C5D9F��6/   $ 1Q�6A   5!      5#      5$      �� 5%      " �6#   .�5&      ��  � �� �	#000000005*      5+      �� 5-      " �6#   .�5.      ��  � �� 5/      �� 50      $	 0.5,1&�62   $
 RelativeToBoundingBox=�6C   $ 0.5,0&�6g   .��51   "   � $ 	#FF094E8D��6/   $ 0Q�6A   � 52   "   $ 	#FF094E8D��6/   $	 0.1Q�6A   � 53   "   $ 	#FF04549F��6/   $
 0.15Q�6A   � 54   "   $ 	#FF0454A1��6/   $	 0.2Q�6A   � 55   "   $ 	#FF0B65AE��6/   $	 0.6Q�6A   � 56   "   $ 	#FF1073BA��6/   $	 0.9Q�6A   � 57   "   $ 	#FF1175BD��6/   $ 1Q�6A   58      5:      5;      5<      ��5?   
   �� 5@      $	 0.5,1&�6"   $
 RelativeToBoundingBox=�63   $ 0.5,0&�6W   .��5A      � $ 	#FF094E8D��6   $ 0Q�61   � 5B      $ 	#FF094E8D��6   $
 0.35Q�61   � 5C      $ 	#FF04549F��6   $
 0.45Q�61   � 5D      $ 	#FF0454A1��6   $	 0.5Q�61   � 5E      $ 	#FF0B65AE��6   $	 0.7Q�61   � 5F      $ 	#FF1073BA��6   $	 0.9Q�61   � 5G      $ 	#FF1175BD��6   $ 1Q�61   5H       � RowDefinitions 5K   
   �� 5L      $��64 �6   �� 5M      $
��AUTO �6   .+�5P   
   � $��0��6    � ColumnDefinitions 5Q      �� 5R      $	��250 �6#   �� 5S      $��4* �6#   �� 5T      $��* �6#    5V      �� 5W      $��24 �6    �� 5X      $��40 �6    ��5[      �� 5\       �� ImageSource$# /Resources/SummaryHeader1.png��6   .+�5a      �� $��0��6   $��1��6(     YOUR_TICKET_Label#���  65   $��28(�6`   $
��Bold"�6n   ���	#FFEEEEEE �� TextAlignment$ Center=�6�   $
��Left=�6�   $��Center=�6�   $��20,5,0,0q�6�    K� ZIndex$ 1��6�   �� 5b      -   5         ��Name$ TxtOverralTripDesc��5b   �   $��1��6   $��1��6(   $��KUALA LUMPUR  >  GEMAS��65   $��24(�6S   $
��Bold"�6a   ���	#FF04549F$ Center=�6�   $
��Left=�6�   $��Center=�6�   $��5,0,0,0q�6�   $ 1��6�   6�   �� 5c      $��3��6   $��0��6$   $��2��61   $ ��/Resources/KTMB_Logo01.png��6B   $ 1��6f   $��Right=�6w   5d      � 5f   
   $��1��6    5g      �� 5h      $
��Auto �6    �� 5i      $
��Auto �6    �� 5j      $
��Auto �6    �� 5k      $
��Auto �6    .+�5m      �� $��0��6   ._�5n      A   ;NssIT.Kiosk.Client.ViewPage.TicketSummary.uscTripTicketSumm  -   5          Name$ TSummDepart1��5n   *   $��0,0,0,2q�6@   $y�Visible=�6Q   5o      �� 5p      $��1��6   ._�5q        -   5        $ TSummDepart2��5q   *   $��0,0,0,2q�6@   $y�	Collapsed=�6Q   5r      �� 5s      $��2��6   ._�5t        -   5        $ TSummReturn1��5t   *   $��0,0,0,2q�6@   $y�Visible=�6Q   5u      �� 5v      $��3��6   ._�5w        -   5        $ TSummReturn2��5w   *   $��0,0,0,2q�6@   $y�	Collapsed=�6Q   5x      5y      5z      5{      