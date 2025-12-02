describe('User Management Page', () => {
  // Mock data representing the API response
  // We include ID 1 (Main Admin) to ensure it is correctly filtered out by the frontend
  const mockUsers = [
    { id: 1, name: 'Main Admin', email: 'admin@system.com', profile: 'ADMIN' },
    {
      id: 10,
      name: 'Alice Citizen',
      email: 'alice@test.com',
      profile: 'CITIZEN',
    },
    { id: 11, name: 'Bob Manager', email: 'bob@test.com', profile: 'MANAGER' },
  ];

  beforeEach(() => {
    // Intercept the GET request to load users
    // Using a wildcard (*) to match query parameters (page, sort, filter)
    cy.intercept('GET', '**/api/user*', {
      statusCode: 200,
      body: mockUsers,
    }).as('getUsers');

    // Mock JWT and User object for authentication
    const jwt =
      'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6ImFkbWluQHN5c3RlbS5jb20iLCJyb2xlIjoiQURNSU4iLCJleHAiOjQ3OTk5OTk5OTksImlhdCI6MTYwOTk5OTk5OX0.signature';
    const adminUser = JSON.stringify({
      id: 999,
      name: 'Cypress Admin',
      email: 'admin@test.com',
      profile: 'ADMIN',
    });

    // Visit the Map page first (safe zone) and inject auth data into LocalStorage
    cy.visit('/map', {
      onBeforeLoad: (window) => {
        window.localStorage.setItem('authToken', jwt);
        window.localStorage.setItem('user', adminUser);
      },
    });

    // Extra wait to ensure Angular processes the login/session restoration
    cy.wait(1000); 

    // Open the sidebar menu
    cy.get('button.toggle-btn').click();

    // Find the "Users" link in the navigation and click it
    // We use a longer timeout here in case the Auth state takes a moment to update the UI
    cy.get('nav a[data-label="Users"]', { timeout: 10000 })
      .should('exist')
      .click();

    // Verify navigation was successful and the API call was made
    cy.url().should('include', '/users');
    cy.wait('@getUsers');
  });

  it('should display the user list correctly', () => {
    // Verify Page Title
    cy.get('h2.um-title').should('contain.text', 'User Management');
    
    // Verify table rows (should have 2 rows because ID 1 is filtered out)
    cy.get('table.um-table tbody tr').should('have.length', 2);
    
    // Verify content of the first visible row
    cy.get('tbody tr')
      .first()
      .within(() => {
        cy.contains('Alice Citizen');
        cy.get('.badge-citizen').should('contain.text', 'CITIZEN');
      });
      
    // Verify that the Main Admin (ID 1) is NOT visible
    cy.contains('Main Admin').should('not.exist');
  });

  it('should filter users by search term', () => {
    // Type in the search box
    cy.get('.filter-input').type('Bob');
    
    // Click Apply
    cy.get('button').contains('Apply').click();
    
    // Verify the API was called with the filter parameter
    cy.wait('@getUsers').then((interception) => {
      expect(interception.request.url).to.include('filter=Bob');
    });
    
    // Click Reset
    cy.get('button').contains('Reset').click();
    
    // Verify the API was called again without the filter
    cy.wait('@getUsers').then((interception) => {
      expect(interception.request.url).to.not.include('filter=Bob');
    });
  });

  it('should open create modal and create a new user', () => {
    // Intercept POST request for creating a user
    cy.intercept('POST', '**/api/user*', {
      statusCode: 200,
      body: {
        id: 99,
        name: 'New Guy',
        email: 'new@guy.com',
        profile: 'MANAGER',
      },
    }).as('createUser');

    // Click the FAB (Floating Action Button) to add a user
    cy.get('.fab-add-user').click();
    
    // Verify Modal is visible
    cy.get('.rt-modal').should('be.visible');

    // Fill out the form
    cy.get('input[name="name"]').type('New Guy');
    cy.get('input[name="email"]').type('new@guy.com');
    cy.get('select[name="profile"]').select('Citizen'); // Selecting by label text
    cy.get('input[name="password"]').type('123456');

    // Click Create
    cy.get('button').contains('Create').click();

    // Verify the request payload matches the inputs
    // Note: Checking specific property casing (Name vs name) based on Service logic
    cy.wait('@createUser').then((interception) => {
      expect(interception.request.body).to.deep.include({
        Name: 'New Guy',
        Profile: 'CITIZEN',
      });
    });

    // Verify Success Toast and list reload
    cy.get('.alert-success').should(
      'contain.text',
      'User created successfully'
    );
    cy.wait('@getUsers');
  });

  it('should edit an existing user', () => {
    // Intercept PUT request for updating a user
    cy.intercept('PUT', '**/api/user/*', { statusCode: 200, body: {} }).as(
      'updateUser'
    );

    // Click the Edit button on the first row
    cy.get('tbody tr').first().find('button[title="Edit"]').click();
    
    // Update the name field
    cy.get('input[name="name"]').clear().type('Alice Updated');
    
    // Click Save
    cy.get('button').contains('Save').click();

    // Verify the payload contains the updated name
    cy.wait('@updateUser').then((interception) => {
      expect(interception.request.body.Name).to.equal('Alice Updated');
    });
    
    // Verify Success Toast
    cy.get('.alert-success').should(
      'contain.text',
      'User updated successfully'
    );
  });

  it('should delete a user after confirmation', () => {
    // Intercept DELETE request
    cy.intercept('DELETE', '**/api/user/*', { statusCode: 200, body: {} }).as(
      'deleteUser'
    );

    // Find the row with 'Bob Manager' and click Delete
    cy.contains('tr', 'Bob Manager').find('button[title="Delete"]').click();
    
    // Verify Confirmation Modal is visible
    cy.get('.rt-modal').should('be.visible');
    
    // Click Delete in the modal
    cy.get('button').contains('Delete').click();

    // Verify the correct ID was deleted (Bob is ID 11 in mock data)
    cy.wait('@deleteUser').then((interception) => {
      expect(interception.request.url).to.include('/11');
    });
    
    // Verify Success Toast and visual removal
    cy.get('.alert-success').should('contain.text', 'removed successfully');
    cy.contains('tr', 'Bob Manager').should('not.exist');
  });

  it('should handle sorting', () => {
    // Click Name header to sort
    cy.contains('th', 'Name').click();
    cy.wait('@getUsers').then((interception) => {
      expect(interception.request.url).to.include('sortBy=Name');
    });

    // Click Email header to sort
    cy.contains('th', 'Email').click();
    cy.wait('@getUsers').then((interception) => {
      expect(interception.request.url).to.include('sortBy=Email');
    });
  });

  it('should handle pagination correctly', () => {
    // Simulate navigation to page 2
    cy.get('button').contains('Next').click();

    // Verify the API was called with pageNumber=2
    cy.wait('@getUsers').then((interception) => {
      expect(interception.request.url).to.include('pageNumber=2');
    });

    // Update the visual counter on the page
    cy.get('.page-counter strong').should('contain.text', '2');

    // Simulate navigation back to page 1
    cy.get('button').contains('Previous').click();

    // Verify the API was called with pageNumber=1
    cy.wait('@getUsers').then((interception) => {
      expect(interception.request.url).to.include('pageNumber=1');
    });
  });

  it('should handle load error gracefully', () => {
    // Simular erro 500 ao carregar utilizadores
    cy.intercept('GET', '**/api/user*', {
      statusCode: 500,
      body: { message: 'Server error' }
    }).as('getUsersError');

    // Forçar recarregamento chamando a função de filtro ou reload da pagina
    cy.visit('/users');
    cy.wait('@getUsersError');

    // Verificar se a mensagem de erro aparece no HTML (baseado no *ngIf="error")
    cy.get('.alert.alert-danger').should('contain.text', 'Failed to load users');
  });

  it('should handle create user error', () => {
    // Open modal
    cy.get('.fab-add-user').click();
    
    // Fill out the form
    cy.get('input[name="name"]').type('Error User');
    cy.get('input[name="email"]').type('error@test.com');
    cy.get('input[name="password"]').type('123');

    // Simulate 400 error on POST
    cy.intercept('POST', '**/api/user*', {
      statusCode: 400,
      body: { message: 'Invalid data' }
    }).as('createUserError');

    cy.get('button').contains('Create').click();
    cy.wait('@createUserError');

    // Verify error Toast
    cy.get('.rt-toast-container .alert-danger').should('contain.text', 'Failed to create user');
    
    // Verify that the modal closes after error
    cy.get('.rt-modal').should('not.exist');
  });

  it('should disable save button if form is invalid', () => {
    cy.get('.fab-add-user').click();

    // Button should be disabled initially (empty fields)
    cy.get('button').contains('Create').should('be.disabled');

    // Fill out only the name -> should remain disabled
    cy.get('input[name="name"]').type('Incomplete User');
    cy.get('button').contains('Create').should('be.disabled');

    // Fill out email -> should remain disabled (password missing in create mode)
    cy.get('input[name="email"]').type('test@test.com');
    cy.get('button').contains('Create').should('be.disabled');

    // Fill out password -> now should be enabled
    cy.get('input[name="password"]').type('123456');
    cy.get('button').contains('Create').should('not.be.disabled');
  });

  it('should display empty state when no users return', () => {
    // Simulate empty API response (excluding ID 1 which is filtered by the front)
    cy.intercept('GET', '**/api/user*', {
      statusCode: 200,
      body: [] // Empty list
    }).as('getEmptyUsers');

    cy.reload();
    cy.wait('@getEmptyUsers');

    // Verify the table disappeared
    cy.get('table.um-table').should('not.exist');

    // Verify the "No users found" message appears
    cy.get('.empty-state').should('be.visible')
      .and('contain.text', 'No users found');
  });
});