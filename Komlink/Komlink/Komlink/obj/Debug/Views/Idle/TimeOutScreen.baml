   M S B A M L   `   `   `  ����B  >Komlink, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:Komlink.Views.IdleKomlink.Views.Idle  5      &     Komlink.Views.Idle.TimeOutScreen   5      ^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      2local clr-namespace:Komlink.Views.Idle     5        � 	Resources  5	       �� MergedDictionaries�� 5
   
    5      �� 5      %    5      .��5      �� ���	#C00000FF �� CornerRadius$ 50��6$   ._�5   
   �  � ColumnDefinitions 5      �� 5      $��20 �6#   �� 5      $��* �6#   �� 5      $��* �6#   �� 5      $��* �6#   �� 5      $��* �6#   �� 5      $��* �6#   �� 5      $��20 �6#    � RowDefinitions 5      �� 5      $��30 �6    �� 5!      $��* �6    �� 5$      $
��0.8* �6    �� 5'      $��2* �6    �� 5*      $
��0.1* �6    �� 5-      $��2* �6    �� 5/      $��30 �6    .+�52      �� -   5         ��Name$ TxtTimeoutMessage��52   �   $��1��6   $��1��6(   $��5��65   $��Center=�6I   $��Center=�6d   $��30(�6�   $
��Bold"�6�   ���	#FFDDDDDD$��TIMEOUT_IN_Label��6�   $��WrapWithOverflow=�6�   �� 54      -   5        $ TxtCountDown��54   �   $��2��6   $��3��6(   $��3��65   $��Center=�6I   $��Center=�6d   $	��156(�6�   $
��Bold"�6�   ���	#FFFBD012$��30��6�   6�   �� 56      $��1��6   $��5��6)   $��5��66    �� Orientation$ 
Horizontal=�6J   $��Bottom=�6c   $��Center=�6~   .+�58      �� -   5         ��Name$ BdExit��58      $ 10��6)   ���	#FFFBD012$��90��6R    � Cursor$
 Hand��6_   $��Bottom=�6m   $��Right=�6�   $	��250��6�   $��0,0,10,0q�6�   ._�5:      �� -   5        $��ExitText��5:       $
��EXIT��61   ���	#FF272727$��Center=�6T   $��Center=�6o   $��36(�6�   $
��Bold"�6�   5;      �� 5=      -   5        $ 
BdContinue��5=      $ 10��6-   ���	#FFFBD012$��90��6V   $��Bottom=�6c   $��Right=�6~   $	��250��6�   $��10,0,0,0q�6�   ._�5?      �� -   5        $��ContinueText��5?       ���	#FF272727$��Center=�6K   $
��Left=�6f   $��20(�6�   $��WrapWithOverflow=�6�   $
��Bold"�6�   	 �� TextAlignment$	 Center=�6�   $#��CONTINUE EXISTING TRANSACTION��6�   $��
26,16,0,16q�6�   $
 Hand��6  5@      5A      5B      5C      5E      