   M S B A M L   `   `   `  ����B  >Komlink, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:Komlink.ViewsKomlink.Views  5      "    Komlink.Views.AlertUnSuccess   5      ^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      -localclr-namespace:Komlink.Views     5        � 	Resources  5	         BorderStyle �� 
TargetType �� Property �� Value  ButtonStyle �� 
TargetType �� Triggers P� Property P� Value��%  &	        &	 @    �� 5
   
   " ��6$   .�5      ��  � ��$	 290��6&   �� 5       � �� �#FBD0126+   �� 5       �@��CornerRadius$ 10��6-   �� 5       � ��$
 Left=�64   �� 5       � ��$ Center=�62   �� 5       � ��$ 83��6'   �� 5       � ��$ 20 0 0 0q�6'   5      �� 5   
   " ��6$   .�5      ��  � ��
 ���� 6+   �� 5       � ��$ 32(�6)   �� 5       � ��$ SemiBold"�6+   �� 5       � ��$ 0q�60   �� 5       � �� 5      �� 5      " ��6&   .R�5      �� 
������ ._�5       �� $��Center=�6/   $��Center=�6L   5"       5#      P� 5$       �@J�IsMouseOver . . �5%   "   ��  � ��
 ���� 6?   5&      5(      5*      5+      5,      .��5/      �� ���#074481 �� CornerRadius$ 15��6"   $	��410��64   $	��940��6A   ._�50   
   � -   5        	 �Name$	 GridContent��50      
 � RowDefinitions
 51      �� 52      �� 53      �� 54      �� 55      .+�57      � $��0��6    � ColumnDefinitions 58      �� 59      $	��600 �6'   �� 5:      .+�5<      �� -   5        $��
AlertTitle��5<   :   $��Top Up Unsuccessfully!��6   ���#fff$��40(�6^   $��Bottom=�6l   $��Right=�6�   $��SemiBold"�6�   $
��Wrap=�6�   �� 5=      $��1��6   $��Bottom=�6)   $��55��6D   $��55��6O   $
��Left=�6[   $��20 0 0 0q�6v   
���  ��$ 30��6�   ._�5>      �� $��/Assets/cancel.png��6   $��55��68   5?      5@      �� 5C      -   5        $��PrintReceiptText��5C   L   $%��Please refer to Komuter counter��6   $��1��6?   ���#fff$��40(�6v   $��Center=�6�   $��Center=�6�   $��SemiBold"�6�   $
��Wrap=�6�    �� TextAlignment$ Center=�6�   �� 5D      -   5         ��Name$ OrText��5D      $��Or��6(   $��2��62   ���#fff$��40(�6R   $��Center=�6`   $��Center=�6{   $��SemiBold"�6�   $
��Wrap=�6�   $ Center=�6�   � 5F      $��3��6   $	��Top=�6     5I      �� 5J      �� 5K      .+�5M      �� -   5        $��ClickAgainButton��5M   k   $��0��6   #��[  6)   $��Center=�6N   ._�5N      �� -   5        $��
Scan Again��5N      #��[ 62   6W   5O      �� 5Q      $��1��6   #��[  6)   $��Center=�6N   ._�5R      �� -   5        $��Ok��5R      #��[ 6*   6O   5S      5U      5X      5Y      5Z      