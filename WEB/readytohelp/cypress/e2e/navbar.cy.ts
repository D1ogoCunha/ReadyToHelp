describe('Navbar Component', () => {

  /**
   * Helper function to expand the sidebar menu if it is currently collapsed.
   * This resolves Cypress errors regarding elements being "not visible" or 
   * "center hidden" by ensuring the navigation links are fully viewable before interaction.
   */
  const ensureMenuOpen = () => {
    cy.get('nav').then(($nav) => {
      if ($nav.hasClass('collapsed')) {
        cy.get('.toggle-btn').click();
        cy.wait(300); // Short wait for the CSS transition to complete
      }
    });
  };

  beforeEach(() => {
    // Ensure a clean state before every test
    cy.clearLocalStorage();
  });

  context('Unauthenticated State (Guest)', () => {
    beforeEach(() => {
      cy.visit('/map');
      ensureMenuOpen(); // Ensure the menu is open so links are visible for assertions
    });

    it('should display guest links and hide authenticated links', () => {
      // Since the menu is open, we can assert standard visibility
      cy.get('a[data-label="Map"]').should('be.visible');
      cy.get('a[data-label="History"]').should('be.visible');

      // Guest specific links
      cy.get('a[data-label="Login"]').should('be.visible');

      // Authenticated specific links (Should NOT exist in DOM)
      cy.get('a[data-label="Users"]').should('not.exist');
      cy.get('a[data-label="Logout"]').should('not.exist');
    });

    it('should navigate to History page when clicked', () => {
      // Click the history link and verify the URL changes correctly
      cy.get('a[data-label="History"]').click();
      cy.url().should('include', '/occurrences/history');
    });

    it('should toggle sidebar and persist state in localStorage', () => {
      // Since beforeEach already opened the menu, we test closing it first

      // 1. Close the sidebar
      cy.get('.toggle-btn').click();
      cy.get('nav').should('have.class', 'collapsed');

      // 2. Re-open the sidebar
      cy.get('.toggle-btn').click();
      cy.get('nav').should('not.have.class', 'collapsed');

      // 3. Verify persistence in localStorage
      cy.getAllLocalStorage().then((result) => {
        // Verify that the specific key exists and is set to '1' (true)
        const ls = result[Cypress.config('baseUrl')!] || result[Object.keys(result)[0]];
        expect(ls['rt-sidebar-open']).to.eq('1');
      });

      // 4. Reload page to verify state memory
      cy.reload();
      cy.get('nav').should('not.have.class', 'collapsed');
    });
  });

  context('Authenticated State (Logged In)', () => {
    beforeEach(() => {
      cy.visit('/map', {
        onBeforeLoad: (window) => {
          // Inject valid Auth data into LocalStorage to simulate a logged-in user
          window.localStorage.setItem('authToken', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6ImFkbWluQHN5c3RlbS5jb20iLCJyb2xlIjoiQURNSU4iLCJleHAiOjQ3OTk5OTk5OTksImlhdCI6MTYwOTk5OTk5OX0.signature');
          window.localStorage.setItem(
            'user',
            JSON.stringify({
              id: 999,
              name: 'Cypress Admin',
              email: 'admin@test.com',
              profile: 'ADMIN',
            })
          );
          // Set sidebar to open by default to avoid click interception issues
          window.localStorage.setItem('rt-sidebar-open', '1');
        },
      });
      
      // Wait for Angular to process the auth state
      cy.wait(500);
      ensureMenuOpen();
    });

    it('should display authenticated links and hide login link', () => {
      // Verify that the Login link has disappeared
      cy.get('a[data-label="Login"]').should('not.exist');

      // Verify that "Users" and "Logout" links are visible
      cy.get('a[data-label="Users"]').should('be.visible');
      cy.get('a[data-label="Logout"]').should('be.visible');
    });

    it('should navigate to Users Management page', () => {
      cy.get('a[data-label="Users"]').click();
      cy.url().should('include', '/users');
    });

    it('should handle logout action correctly', () => {
      cy.get('a[data-label="Logout"]').click();

      // Verify that the UI reverts to the Guest state
      cy.get('a[data-label="Login"]').should('be.visible');
      cy.get('a[data-label="Users"]').should('not.exist');
    });
  });
});