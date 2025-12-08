import YAML from 'js-yaml';

export async function loadConfig() {
  const res = await fetch('/config/appSettings.yaml', {cache: 'no-store'});
  if (!res.ok) throw new Error('Could not load config: ' + res.status);
  const text = await res.text();
  const cfg = YAML.load(text);
  return cfg;
}