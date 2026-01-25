document.addEventListener('click', (e) => {
  const btn = e.target.closest('#searchBtn');
  if (!btn) return;
  e.preventDefault();
  executeSearch();
});

function executeSearch() {
  const rawValue = document.getElementById('company')?.value;
  const cleanQuery = rawValue?.trim() || '';
  if (!cleanQuery) return;

  const encodedQuery = encodeURIComponent(cleanQuery);
  const googleUrl = `https://www.google.com/search?q=${encodedQuery}`;
  const newWindow = window.open(googleUrl, '_blank');
  if (newWindow) newWindow.focus(); else window.location.href = googleUrl;
}