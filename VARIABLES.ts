type MonorepoFolderKey =
  | 'root'
  | 'client'
  | 'server'
  | 'shared'
  | 'docs'
  | 'assets';

type MonorepoFolders = Record<MonorepoFolderKey, string>;

type CanonicalVariable =
  | '[MOBILE]'
  | '[GAIN MONEY]'
  | '[THRALL]'
  | '[PLAYER]'
  | '[VAMPIRE]'
  | '[WEREWOLF]'
  | '[HORDE]'
  | '[CLAN]'
  | '[BATTLEFIELD]'
  | '[NPC]'
  | '[DEATH]'
  | '[STRONGER]'
  | '[TRHALL]'
  | '[DUSKEN COIN]'
  | '[BLOOD SHARDS]'
  | '[PREMIUM]'
  | '[ASHEN ONE]';

const MONOREPO_FOLDERS: MonorepoFolders = {
  root: 'VAMPIRES',
  client: 'client',
  server: 'server',
  shared: 'packages/shared',
  docs: 'Docs',
  assets: 'assets'
};

const CANONICAL_VARIABLES: readonly CanonicalVariable[] = [
  '[MOBILE]',
  '[GAIN MONEY]',
  '[THRALL]',
  '[PLAYER]',
  '[VAMPIRE]',
  '[WEREWOLF]',
  '[HORDE]',
  '[CLAN]',
  '[BATTLEFIELD]',
  '[NPC]',
  '[DEATH]',
  '[STRONGER]',
  '[TRHALL]',
  '[DUSKEN COIN]',
  '[BLOOD SHARDS]',
  '[PREMIUM]',
  '[ASHEN ONE]'
];

type GlobalVariables = {
  folders: MonorepoFolders;
  canonical: readonly CanonicalVariable[];
};

const VARIABLES: GlobalVariables = {
  folders: MONOREPO_FOLDERS,
  canonical: CANONICAL_VARIABLES
};

export type {
  CanonicalVariable,
  GlobalVariables,
  MonorepoFolderKey,
  MonorepoFolders
};
export { CANONICAL_VARIABLES, MONOREPO_FOLDERS, VARIABLES };

