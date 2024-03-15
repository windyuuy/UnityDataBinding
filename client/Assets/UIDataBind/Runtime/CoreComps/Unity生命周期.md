
OnTransformChildrenChanged：只能监听浅层子节点的变化
- 无论父子节点是否active，父节点都能监听到变化
- 可以监听到通过 Instantiate, setparent 造成的子节点变化
- 无法监听直接反序列化出的子节点新增事件

OnTransformParentChanged：可以递归监听所有父节点变化
- 无论父子节点是否active，都能监听到递归父节点的变化
- 可以监听到setparent造成的父节点变化，无法监听通过instantiate造成的父节点设置
- 无法监听直接反序列化出的父节点设置事件

ISerializationCallbackReceiver
	- OnAfterDeserialize
	- OnBeforeSerialize

PlayerLoop
