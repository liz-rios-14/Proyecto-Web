export default function KeyboardHelp({ title = "Atajos", shortcuts = [] }) {
  if (shortcuts.length === 0) return null;

  return (
    <div className="keyboard-help">
      <p className="keyboard-help-title">{title}</p>

      {shortcuts.map((shortcut) => (
        <p key={shortcut}>{shortcut}</p>
      ))}
    </div>
  );
}