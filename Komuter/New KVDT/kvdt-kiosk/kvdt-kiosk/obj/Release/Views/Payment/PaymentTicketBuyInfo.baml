   M S B A M L   `   `   `  ����   
kvdt-kiosk3    -kvdt_kiosk.Views.Payment.PaymentTicketBuyInfo   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     -   $��Inter)�  � 	Resources   �� 
TargetType �� Property �� Value �� 
TargetType � Cursor �� Padding �� CornerRadius��%|   '��       �� " ��.���  � �� �� " ��.R��� 
������ $��0q�$
 Hand��$ 5q�$ 12��
�������._��� .��� .+��� -    ��Name$ ProcessingTextBlock��$��Center=�$��Center=�$y�Visible=�$��Processing...��$
��Bold"�$��50(�
�������� -   	 �Name$	 CurrentGrid��$y�Hidden=�.+�� $��	25,5,25,5q�
 � RowDefinitions
 �� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto ��� $��* �.+��� -   $ 
lblPayment��$��PAYMENT��$��60(�$
��Wrap=����#FFFFFF$��Segoe UI)�$
��Bold"��� -   $ lblStationInfo��$$��From And To Station Infomation��$
��Bold"�$��WrapWithOverflow=�$��1��$��18(����#FFFFFF$��0,5,0,5q��� -   $ lblDateTypeJourneyInfo��$��Date And Type Of Journey��$��2��$��18(����#FFFFFF$��WrapWithOverflow=�$
��Bold"�$��0,5,0,5q�A� -    A�Name$ 
GridTicket��$��3��$��	0,15,0,15q�A� -   $ 
GridParcel��$��6���� -	   $ lblTotalTicketPrice��$��MYR 0.00��$��7��$��55(����#FFFFFF$��	0,27,10,5q�$��Right=�$��Segoe UI)�$
��Bold"�A� $��8�� A� Rows$ 2�� A� Columns$ 2��$��0,25,0,5q�.+��� -
   $��8q�
�������$ 10��$��90��$y�Visible=�._�� 
 �� $��20 ��� $��* ��� $��10 �.+��� -   $ TxtPaymentDesc��$
��Card��$��11(�$��Center=�$��Center=�$
��Bold"��� -    ��Name$ imgPaymentType��$��1��$*��$/Assets/Master_Visa_Debit_Card01.png��$��Center=�
 � Tag$  ���� -   $��8q�
�������$ 10��$��90��$y�Visible=�._�� 
 �� $��20 ��� $��* ��� $��10 �.+��� $��Boost��$��11(�$��Center=�$��Center=�$
��Bold"��� $��1��$��/Assets/Boost.jpg��$��Center=�$  ��$
��Fill=�$��45��$	��100���� -   $��8q�
�������$ 10��$��90��$y�Visible=�._�� 
 �� $��20 ��� $��* ��� $��10 �.+��� $��
Touch N Go��$��11(�$��Center=�$��Center=�$
��Bold"��� $��1��$��/Assets/TNC.jpg��$��Center=�$  ��$
��Fill=�$��45��$	��100���� -   $ lblAttention��$2��,PLEASE CHECK YOUR SELECTION BEFORE CLICK PAY��$
��Bold"�$��WrapWithOverflow=�$��9��$��Center=�$��Center=�$��35(����#FBD012$��0,35,0,0q�