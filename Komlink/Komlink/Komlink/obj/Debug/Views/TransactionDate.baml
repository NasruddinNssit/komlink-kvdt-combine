   M S B A M L   `   `   `  ����B  >Komlink, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:Komlink.ViewsKomlink.Views  5      #    Komlink.Views.TransactionDate   5      ^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      -localclr-namespace:Komlink.Views     5      $��Roboto)�5        � 	Resources  5
         BorderStyle �� 
TargetType �� Property �� Value  ButtonStyle �� 
TargetType �� Triggers P� Property P� Value��%�  &	        &	 �     �� 5   
   " ��6$   .�5      ��  � �� �#0744816+   �� 5       �@��CornerRadius$ 10��6-   �� 5       � ��$ 60��6'   �� 5       � ��$	 200��6&   �� 5       � ��$ 	20 0 20 0q�6'   5      �� 5   
   " ��6$   .�5      ��  � ��
 �����6+   �� 5       � ��$ 25(�6)   �� 5       � �� 5      �� 5      " ��6&   .R�5      �� 
������ ._�5      �� $��Center=�6/   $��Center=�6L   5       5      P� 5       �@J�IsMouseOver . . �5   "   ��  � ��
 ���� 6?   5      5!      5#      5$      5%      .��5&      �  � RowDefinitions 5'   
   �� 5(      $��10 �6   �� 5)      $	��200 �6   �� 5*      $	��100 �6   .+�5,   
   �� $��	20 0 20 0q�6   
���   �6'   � 5-   
   $��1��6   .+�5.      �� $��1q�6   
�������	 �� CornerRadius$	 10��6<   $��Center=�6N   $	��550��6i   
 �� Padding$
 20q�6w   ._�5/      �  � ColumnDefinitions 50      �� 51      �� 52      $��50 �6+   �� 53      .+�56      � $��0��6    57      �� 58      $��50 �6,   �� 59      .+�5;      �� -   5        $��DateFromText��5;   �   $��	Date From��6$   $��25(�65   $��SemiBold"�6C    �� TextAlignment$ Center=�6Y   $��Center=�6p   6�   �� 5<      -   5        $��	StartDate��5<   �   $��1��6$   $��30 Nov 2023��61   $��35(�6D   $
��Bold"�6R   $��Center=�6d   $��Center=�6   6�   5=      �� 5?      $��1��6   $��1q�6-   
�������$��Center=�6T   $��Center=�6o   $	��100��6�   � 5A      $��2��6    5B      �� 5C      $��50 �6,   �� 5D      .+�5F      �� -   5        $��
DateToText��5F   �   $��Date To��6$   $��25(�63   $��SemiBold"�6A   $ Center=�6W   $��Center=�6n   6�   �� 5G      -   5        $��EndDate��5G   �   $��1��6$   $��
1 Dec 2023��61   $��35(�6C   $
��Bold"�6Q   $��Center=�6c   $��Center=�6~   6�   5H      5I      5J      5K      � 5L   
   -   5        $��DateOptionGrid��5L      $��2��6   5N      5O      5P      