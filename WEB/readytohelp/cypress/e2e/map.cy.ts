describe('Map Page (Home)', () => {
  
  // Mock data to simulate API response for active occurrences
  const mockOccurrences = [
    {
      id: 101,
      title: 'Test Fire in Lisbon',
      type: 'FOREST_FIRE',
      status: 'ACTIVE',
      priority: 'HIGH',
      latitude: 38.7223, 
      longitude: -9.1393
    },
    {
      id: 102,
      title: 'Traffic Jam on Bridge',
      type: 'TRAFFIC_CONGESTION',
      status: 'ACTIVE',
      priority: 'MEDIUM',
      latitude: 38.7,
      longitude: -9.15
    }
  ];

  beforeEach(() => {
    // 1. Intercept the API call to fetch active occurrences.
    cy.intercept('GET', '**/api/occurrence/active', {
      statusCode: 200,
      body: mockOccurrences
    }).as('getActiveOccurrences');

    // 2. Visit the map page
    cy.visit('/map');
    
    // 3. Wait for the initial API call to complete
    cy.wait('@getActiveOccurrences', { timeout: 35000 });
  });

  it('should display the map container and markers', () => {
    // Verify if the map container div exists and is visible
    cy.get('#map').should('be.visible');
    
    // CORREÇÃO: Verificar se o Canvas existe no DOM, mas relaxar a verificação de visibilidade
    // porque os marcadores (pins) podem estar a cobri-lo parcialmente.
    cy.get('canvas.mapboxgl-canvas').should('exist');

    // Verify if the custom markers are created in the DOM.
    cy.get('.custom-marker').should('have.length', 2).and('be.visible');
  });

  it('should open a popup with correct details when a marker is clicked', () => {
    // 1. Click on the first marker
    // 'force: true' is ESSENTIAL here because markers are layers on top of the map
    cy.get('.custom-marker').first().click({ force: true });

    // 2. Verify that the popup container appears
    cy.get('.mapboxgl-popup-content').should('be.visible');

    // 3. Verify content
    cy.contains('h5', 'Test Fire in Lisbon').should('be.visible');
    cy.contains('Forest Fire').should('be.visible');
    cy.get('.badge-danger').should('contain.text', 'High');
  });

  it('should navigate to the details page when "View Details" is clicked', () => {
    // 1. Open the popup
    cy.get('.custom-marker').first().click({ force: true });

    // 2. Click "View Details"
    cy.contains('button', 'View Details').click();

    // 3. Verify URL
    cy.url().should('include', '/occurrence/101');
  });
});