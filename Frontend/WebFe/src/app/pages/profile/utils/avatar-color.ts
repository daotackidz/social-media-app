/** Same placeholder-avatar palette style used by the mock feed/story data elsewhere in the app. */
const PALETTE = [
  '#405de6', '#dc2626', '#2563eb', '#059669', '#9333ea',
  '#ea580c', '#0891b2', '#be185d', '#334155', '#7c2d12'
];

/** Deterministic color for a username/id, so the same user always gets the same placeholder color. */
export function avatarColorFor(seed: string): string {
  let hash = 0;
  for (let i = 0; i < seed.length; i++) {
    hash = (hash * 31 + seed.charCodeAt(i)) >>> 0;
  }
  return PALETTE[hash % PALETTE.length];
}

export function avatarInitialFor(name: string): string {
  return (name || '?').charAt(0).toUpperCase();
}
