   M S B A M L   `   `   `  ����D  @SBUpdater, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null9!clr-namespace:SBUpdater.ModelVievSBUpdater.ModelViev  5   
   "    SBUpdater.Viev.ConfigureAttr   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   3model!clr-namespace:SBUpdater.ModelViev     5   
   h dMicrosoft.Expression.Interactions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35Bei9http://schemas.microsoft.com/expression/2010/interactions  5   	   c _System.Windows.Interactivity, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35Bi:http://schemas.microsoft.com/expression/2010/interactivity  5   	     � Title$  ConfigureAttr��5   	   $	��300��6   $	��600��6,   .��5      �  � RowDefinitions 5	   
   �� 5
      $��* �6   �� 5      $��50 �6   �� 5      $��50 �6   .+�5   
   �� $��0��6   .+�5      �� $	��500��6\   $��25��6h   $��5q�6t   ��6   �� *CurrentAttrName+ �� UpdateSourceTrigger$ PropertyChanged=�6t   �� 5       �� Orientation$ 
Horizontal=�6   $
��Left=�62   $	��Top=�6M   .+�5      �� $	��200��6E   $��5q�6Q   $��25��6\   ��6   �� *CurrentAttrNames+ �� SelectedItem 6h   �� *
AttrName+$ PropertyChanged=��� 5      $	��150��6B   $��25��6N   $��5q�6Z   ��6   �� *AttrGroupNames+ 6e   �� *AttrGroupName+5      5      �� 5   
   $ 
Horizontal=�6   $��1��6.   .+�5      �� $'��!Добавить Аттрибут��6   $��10q�6R   $��5q�6^   ��61   �� *AddAttribute+6^   �� 5      $6��0Добавить группу атрибутов��6   $��5q�6_   ��69   �� *AddAttributeGroup+6_   5      �� 5   
   $��2��6   $��Сохранить��6   $��10q�6O   $��Right=�6\   ��62   �� *
SaveAttr+6\   5      5      