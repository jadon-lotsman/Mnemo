export interface ContextMenuOption {
  icon: string
  label: string
  selected?: boolean
  disabled?: boolean
  action: () => void
}
